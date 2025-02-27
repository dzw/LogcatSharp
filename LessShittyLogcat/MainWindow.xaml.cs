﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


// Hi!
// Unfortunately I had just 2 days to finish this, document it and get it out the door
// As such there's a strange mishmash of code and obsolete naming conventions.
// This will get some love and attention going forward, thanks.
// 


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;
using System.Linq;
using System.Web;
using System.Text;

namespace LessShittyLogcat {
	
	// Can a message match any filter, or all?
	public enum FilterMode{ ANY, ALL }

	// Stretch the final column to the width of its content?
	// or stretch the final column to the width of the parent Listview?
	public enum ListViewResizeType{ TOCONTENT, TOPARENT };

	public class FilterGroup{		
		public CheckBox checkBox;
		public TextBox textBox;
		public bool isExclusion;
		public bool isEnabled => checkBox.IsChecked.GetValueOrDefault();
		public string text => textBox.Text;
		public override string ToString()
		{
			return "GRP " + checkBox + " / " + textBox + "(" + text + ")";
		}
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		//public event PropertyChangedEventHandler PropertyChanged;
		
		const int MAX_FILTERS = 4;
		// the integer-based checkboxes
		int wrapLength = 0;		
		int bufferSize = 1000;
		public string WrapString( string inString ){
			
			if ( wrapLength == 0 ) return inString;
			if ( inString.Length < wrapLength ) return inString;

			// floor to int
			int numDivisions = inString.Length / wrapLength;
			for( int i = 0; i < numDivisions; i++ ){
				int insertPoint = (wrapLength * (i+1));
				inString = inString.Insert( insertPoint, "\n" );
			}
			return inString;

		}

		public List<FilterGroup> inclusionGroup = new List<FilterGroup>();
		public List<FilterGroup> exclusionGroup = new List<FilterGroup>();
		public List<FilterGroup> allGroups = new List<FilterGroup>();  // inclusiongroup + exclusiongroup

        public LevelFilterMode levelFilter = LevelFilterMode.Info | LevelFilterMode.Debug | LevelFilterMode.Warning |
                                             LevelFilterMode.Error | LevelFilterMode.Assert;

		private bool FilterEnabled( int which ){ return inclusionGroup[which].isEnabled; }
		private string FilterAt( int which ){ return inclusionGroup[which].text; }

		private bool ExclusionEnabled( int which ){ return exclusionGroup[which].isEnabled; }
		private string ExclusionAt( int which ){ return exclusionGroup[which].text; }

		// Cache the value to cut down on per-line checks
		private bool _anyFilterEnabled = false;
		private bool anyFilterEnabled{
            get {
			    for (int i = 0; i < MAX_FILTERS; i++){
				    if (FilterEnabled(i) && !string.IsNullOrEmpty( FilterAt(i) ))
				    {
					    _anyFilterEnabled = true;
					    return true;
				    }
			    }

                if (levelFilter != LevelFilterMode.Verbose)
                {
                    _anyFilterEnabled = true;
                    return true;
                }
			    _anyFilterEnabled = false;
				return false;
			}
		}


		// Require all inclusions, or require any inclusions?
		public FilterMode filterMode = FilterMode.ANY;

		// Animate the filtered/unfiltered panel width
		public double splitterTarg = 0;
		public double splitterLerp = 50;
		public double gDelta => splitterTarg - splitterLerp;
		public bool animating => Math.Abs(gDelta) > 2;
		
		// Pour la dispatcher
		public delegate void GenericDelegate();
		public delegate void UpdateDelegate(Process logcatProcess);

		// Might as well keep it on the heap
		Process ProcessAdb;



		public static bool finishedLoading = false;

		public MainWindow()
		{
			InitializeComponent();

			// Could over engineer this for the sake of 6 lines.
			// ... or type it out.

			AddGroup( new FilterGroup(){ checkBox=cbFilter1, textBox=txtFilter1, isExclusion=false } );
			AddGroup( new FilterGroup(){ checkBox=cbFilter2, textBox=txtFilter2, isExclusion=false } );
			AddGroup( new FilterGroup(){ checkBox=cbFilter3, textBox=txtFilter3, isExclusion=false } );
			AddGroup( new FilterGroup(){ checkBox=cbFilter4, textBox=txtFilter4, isExclusion=false } );

			AddGroup( new FilterGroup() { checkBox=cbExclude1, textBox=txtExclude1, isExclusion=true } );
			AddGroup( new FilterGroup() { checkBox=cbExclude2, textBox=txtExclude2, isExclusion=true } );

			UpdateFilters(null,null);

			Dispatcher.BeginInvoke( new GenericDelegate( Animate ) );
			
			// to avoid a null PropertyChanged event
			this.DataContext = this;

			AddUnfiltered(new LogEntry() { time="Nowish", text = "Less Shitty Logcat is open source, MPL2.0 licensed. ", color = Colors.DarkBlue });
			AddUnfiltered(new LogEntry() { time="Nowish", text = "See github.com/JonathanDotCel for more info! ", color = Colors.DarkBlue });

			finishedLoading = true;

			logPraser = new LogPraser();
		}
		LogPraser logPraser;


		public void AddGroup(FilterGroup inGroup)
		{

			allGroups.Add(inGroup);

			if (inGroup.isExclusion)
				exclusionGroup.Add(inGroup);
			else
				inclusionGroup.Add(inGroup);
		}

		public LogEntry AddUnfiltered( LogEntry inEntry, bool refreshImmediately = true ){
			listBox1.Items.Add( inEntry );
			if ( refreshImmediately ) 
				RefreshViews();
			return inEntry;
		}

		public LogEntry AddFiltered( LogEntry inEntry, bool refreshImmediately = true ){
			listBox2.Items.Add(inEntry);
			if ( refreshImmediately )
				RefreshViews();
			return inEntry;
		}

		// MVVM would be a wee bit overkill here...
		private void CBEnabled_Clicked(object sender, RoutedEventArgs e)
		{
			
			if ( cbEnabled.IsChecked.GetValueOrDefault() ){
				
				cbPaused.IsChecked = false;
				cbPaused.IsEnabled = true;

				// Clear the buffer or you'll get buffer entries from ~2 hours ago
				// even on terrible old devices like the Galaxy Tab4. Ever wondered where the RAM was going?

				// Logcat -c will plain not work on some devices.
				// "all" (/dev/log/all) is missing on some devices.
				// "main" may be locked.
				BTNDeviceClear_Click(null, null);
				
				// default -b main -b system
				ProcessAdb = LogcatWithParams("logcat -v threadtime");
				ProcessAdb.OutputDataReceived += new DataReceivedEventHandler( logPraser.STDOut_OnDataReceived );
				ProcessAdb.BeginOutputReadLine();
				
				// Get the update pump started!
				cbEnabled.Dispatcher.BeginInvoke(				
					DispatcherPriority.Normal, new UpdateDelegate( Update ), ProcessAdb
				);
				
			} else {
				Console.WriteLine( "Closing process..." );
				cbPaused.IsChecked = true;
				cbPaused.IsEnabled = false;
				if ( !ProcessAdb.HasExited ){
					ProcessAdb?.Kill();
				}
			}

		}

		//  For clearing the logs or starting the main process
		Process LogcatWithParams( string inParams ){
			
			try{
				Process process = new Process();
				process.StartInfo.FileName = "adb";
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.Arguments = inParams;
				process.StartInfo.RedirectStandardInput = true;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.UseShellExecute = false;			
				process.Start();
				AddUnfiltered( new LogEntry(){ text = "Running adb with params: " + inParams, color = Colors.DarkBlue } );
				return process;
			} catch( Exception ) {
				MessageBox.Show( 
					"Woops! Looks like we couldn't find ADB.\n" +
					"Make sure it's on your %PATH%!\n" +
					"Exiting..." 
				);
				System.Environment.Exit(0);
				return null;
			}

		}

		// Obvious, intuitive visual feedback that there are 2 list boxes
		public void Animate()
		{

			if (animating)
			{
				splitterLerp += (splitterTarg - splitterLerp) * 0.4f;
				Thread.Sleep(1000 * 1 / 60);

				secondGrid.ColumnDefinitions[1].Width = new GridLength(splitterLerp, GridUnitType.Star);
				secondGrid.ColumnDefinitions[2].Width = new GridLength(5);
				secondGrid.ColumnDefinitions[3].Width = new GridLength(100 - splitterLerp, GridUnitType.Star);

			}
			else
			{
				splitterLerp = splitterTarg;
			}

			Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new GenericDelegate(Animate));

		}


		// ON UI THREAD
		// Dumps the processed logs into the ListBox on the main Thread
		// see: ParseLog()
		public void Update( Process inProcess ){
			
			if ( logPraser.pendingLogs.Count != 0 && !cbPaused.IsChecked.GetValueOrDefault() ){
				
				// Count them up and scroll in a oner
				// Otherwise you risk introducing small errors
				int addedTo1 = 0;
				int addedTo2 = 0;

				for( int i = 0; i < logPraser.pendingLogs.Count; i++ ){

					LogEntry l = logPraser.pendingLogs[0];
					logPraser.pendingLogs.RemoveAt( 0 );

					// Is it excluded (from both lists)
					if (FilterMatches(exclusionGroup[0], l, false)) break;
					if (FilterMatches(exclusionGroup[1], l, false)) break;
					
					{
						AddUnfiltered( l, false );
						addedTo1++;						
					}

					if ( ValidEntry( l ) )
					{
						AddFiltered( l, false );
						addedTo2++;						
					}

					if (logPraser.pendingLogs.Count == 0)
					{
						break;
					}


				}

				Prune( listBox1 );
				Prune( listBox2 );

				if ( addedTo1 > 0 || addedTo2 > 0 )
					RefreshViews();

				Scroll(listBox1, addedTo1);
				Scroll(listBox2, addedTo2);

			}

			if (ProcessAdb.HasExited && cbEnabled.IsChecked.GetValueOrDefault())
			{
				cbEnabled.IsChecked = false;
				LogEntry l = new LogEntry(){ text = "ADB terminated with exit code " + inProcess?.ExitCode, color = Colors.DarkBlue };
				AddUnfiltered( l );				
			}

			if ( cbEnabled.IsChecked.GetValueOrDefault() ){
				Application.Current.Dispatcher.BeginInvoke(
					DispatcherPriority.ApplicationIdle, new UpdateDelegate(Update), inProcess
				);
			}
			
			// Garbage collector's kinda great, huh?			
			lblBuffer.Content = "Unfiltered buffer: " + listBox1.Items.Count + "/" + bufferSize + " Filtered buffer: " + listBox2.Items.Count + "/" + bufferSize;

		} //Blah

		// Should this LogEntry be added to the filtered list?
		bool ValidEntry( LogEntry inEntry ){

			// Don't go into filtered if there's nothing filtered
			if ( !_anyFilterEnabled ) return false;

            if (!CheckLogLevel(inEntry))
                return false;

            if ( filterMode == FilterMode.ALL ){
				// Simpler to unroll this while we just use 4 filters
				return ( 
					FilterMatches( inclusionGroup[0], inEntry, true )
					&& FilterMatches( inclusionGroup[1], inEntry, true )
					&& FilterMatches( inclusionGroup[2], inEntry, true )
					&& FilterMatches( inclusionGroup[3], inEntry, true )
				);
			} 

			if ( filterMode == FilterMode.ANY ){
				// Simpler to unroll this while we just use 4 filters				
				if (FilterMatches( inclusionGroup[0], inEntry )) return true;				
				if (FilterMatches( inclusionGroup[1], inEntry )) return true;				
				if (FilterMatches( inclusionGroup[2], inEntry )) return true;
				if (FilterMatches( inclusionGroup[3], inEntry )) return true;
			}

            if (levelFilter != LevelFilterMode.Verbose)
                return true;
			
			return false;

		}

        // Checks a given log entry against the Inclusion filters or the Exclusion filters
        // FilterMode.ANY: return false if unchecked
        // FilterMode.ALL: return true if unchecked
        bool FilterMatches( FilterGroup inGroup, LogEntry inEntry, bool uncheckedReturnValue = false ){
			
			// this filter isn't active, so it's not blocking anything
			if ( !inGroup.isEnabled ) return uncheckedReturnValue;
			
			string userFilter = inGroup.text;

			// User's searching for level
			int idx = userFilter.IndexOf("level:",StringComparison.OrdinalIgnoreCase);
			if ( idx == 0 && !string.IsNullOrEmpty( inEntry.level ) ){
				return inEntry.level.IndexOf( userFilter.Substring( 0, 5 ) ) > -1;
			}

			idx = userFilter.IndexOf("pid:", StringComparison.OrdinalIgnoreCase);
			if ( idx == 0 && !string.IsNullOrEmpty( inEntry.PID ) ){				
				return inEntry.PID.IndexOf( userFilter.Substring( 4, userFilter.Length -4 ) ) > -1;
			}

			idx = userFilter.IndexOf("tid:", StringComparison.OrdinalIgnoreCase);
			if ( idx == 0 && !string.IsNullOrEmpty( inEntry.TID ) ){				
				return inEntry.TID.IndexOf( userFilter.Substring( 4, userFilter.Length -4 ) ) > -1;
			}

			idx = userFilter.IndexOf("app:", StringComparison.OrdinalIgnoreCase);
			if ( idx == 0 && !string.IsNullOrEmpty( inEntry.tag ) ){
				return inEntry.app.IndexOf( userFilter.Substring( 4, userFilter.Length -4 ) ) > -1;
			}

			idx = userFilter.IndexOf("tag:", StringComparison.OrdinalIgnoreCase);
			if ( idx == 0 && !string.IsNullOrEmpty( inEntry.tag ) ){
				return inEntry.tag.IndexOf( userFilter.Substring( 4, userFilter.Length -4 ) ) > -1;
			}

			idx = userFilter.IndexOf("text:", StringComparison.OrdinalIgnoreCase);
			if ( idx == 0 && !string.IsNullOrEmpty( inEntry.text ) )
			{				
				return inEntry.text.IndexOf( userFilter.Substring( 5, userFilter.Length -5) ) > -1;
			}

			if ( string.IsNullOrEmpty( inEntry.raw ) )
				return false;
			
			idx = inEntry.raw.IndexOf( userFilter, StringComparison.OrdinalIgnoreCase );			
			return ( idx > -1 );
			
		}

        private bool CheckLogLevel(LogEntry inEntry)
        {
            if (levelFilter == LevelFilterMode.Verbose)
                return true;
            if (!string.IsNullOrEmpty(inEntry.level))
            {
                switch (inEntry.level.Trim().ToLower())
                {
                    case "d":
                        return levelFilter.HasFlag(LevelFilterMode.Debug);
                    case "i":
                        return levelFilter.HasFlag(LevelFilterMode.Info);
                    case "w":
                        return levelFilter.HasFlag(LevelFilterMode.Warning);
                    case "e":
                        return levelFilter.HasFlag(LevelFilterMode.Error);
                    case "a":
                        return levelFilter.HasFlag(LevelFilterMode.Assert);
                }
            }

            return false;
        }


        // Lots of individual scroll calls will be lost in the chop
        // so count them up and apply in one shot where possible
        void Scroll( ListBox inBox, int howManyLines = 1 ){

			if ( howManyLines == 0 ) return;

			if ( cbScroll.IsChecked.GetValueOrDefault() ){
							
				GetScroll( inBox ).ScrollToBottom();

			} else {
				
				GetScroll( inBox ).ScrollToVerticalOffset( GetScroll(inBox).VerticalOffset - howManyLines );
								
			}

		}

		// The hoops we have to jump through with WPF sometimes...
		ScrollViewer GetScroll( ListBox inBox ){			
			Decorator border = (Decorator)VisualTreeHelper.GetChild( inBox, 0 );			
			ScrollViewer sv = (ScrollViewer)VisualTreeHelper.GetChild( border, 0 );
			return sv;			
		}

		void Prune( ListBox inBox ){
			while( inBox.Items.Count > bufferSize ){
				inBox.Items.RemoveAt( 0 );
			}
		}

		// Clear either or both list boxes
		private void BTNClear_Click(object sender, RoutedEventArgs e)
		{
			if ( !anyFilterEnabled || cbSplitView.IsChecked.GetValueOrDefault() ) listBox1.Items.Clear();
			if ( anyFilterEnabled || cbSplitView.IsChecked.GetValueOrDefault()) listBox2.Items.Clear();
		}

		// Not all will succeed on every devices
		// so separate processes get the job done
		private void BTNDeviceClear_Click(object sender, RoutedEventArgs e ){
			LogcatWithParams("logcat -b main -c");
			LogcatWithParams("logcat -b system -c");
			LogcatWithParams("logcat -b radio -c");
			LogcatWithParams("logcat -b events -c");
			LogcatWithParams("logcat -b all -c");
		}
		

		private void UpdateFilters(object sender, RoutedEventArgs e){
			
			// update the cache
			// TODO: not this
			_anyFilterEnabled = anyFilterEnabled;
			
			if ( cbSplitView.IsChecked.GetValueOrDefault() ){				
				splitterTarg = 50;
			} else {				
				splitterTarg = anyFilterEnabled ? 2 : 98;
			}

		}

		
		private void CheckBox_Click(object sender, RoutedEventArgs e)
		{
			filterMode = ( filterMode == FilterMode.ANY ? FilterMode.ALL : FilterMode.ANY );
		}

		// Would be nice if WPF had chosen GridLength instead of "double" for GridViewColumn.Widths		
		private void ListViewSizeChanged(object sender, SizeChangedEventArgs e)
		{			
			RefreshViews();
		}
		
		private void ListViewScrolled( object sender, EventArgs  e ){
			cbScroll.IsChecked = false;
		}

		// Turns the filter boxes off only when a change is detected
		// saves filtering printable/nonprintable chars
		private void Filterboxes_TextChanged(object sender, TextChangedEventArgs e)
		{
			
			// Called during init, crashy
			if ( !finishedLoading ) return;

			TextBox whichBox = (TextBox)sender;

			// Still feels dirty using Linq for this...
			FilterGroup f =
				(from grp in allGroups
				 where grp.textBox == whichBox
				 select grp).FirstOrDefault();

			f.checkBox.IsChecked = false;
			UpdateFilters( null, null );
			
		}

		// disable filter boxes while typing
		// until the user hits Enter
		private void txtFilter_KeyUp(object sender, KeyEventArgs e)
		{
			TextBox whichBox = (TextBox)sender;
			
			// I know I just said this, but it feels dirty using linq for this.
			FilterGroup f = 
				( from grp in allGroups
				where grp.textBox == whichBox
				select grp).FirstOrDefault();
			
			bool isEnter = e.Key == Key.Enter;
			bool empty = string.IsNullOrEmpty(whichBox.Text);

			if ( isEnter && !empty ){
				f.checkBox.IsChecked = true;
				UpdateFilters( null, null );
			}

		}


		// Limits the input to integers, but doesn't (yet) validate ranges.
		// Remember in like 1999, Delphi, Visual Basic, etc had this boilerplate busywork covered?
		public void IntegerLimit( TextBox inBox, int defaultValue ){
			
			// called during initialisecomponent
			if ( inBox == null ) return;

			if ( !int.TryParse( inBox.Text, out int dummyValue ) ){
				int oldSelection = inBox.SelectionStart;
				string newString = "";
				for( int i = 0; i < inBox.Text.Length; i++ ){
					if ( (byte)inBox.Text[i] >= '0' && (byte)inBox.Text[i] <= '9' )
						newString += inBox.Text[i];
				}
				if ( string.IsNullOrEmpty( newString ) )
					newString = defaultValue.ToString();
				
				// Put it back 'cause we're about to re-parse
				inBox.Text = newString;

				// E.g. when pasting over the existing value
				inBox.SelectionStart = Math.Min( oldSelection, newString.Length );
			}
		}

		// 0 for no max
		public int FinaliseTextbox( TextBox inBox, int inMin, int inMax ){

			// guaranteed to be a parsable int already via IntegerLimit() callbacks
			
			// parse the new value
			int returnVal;
			int.TryParse( inBox.Text, out returnVal );

			bool updateSelection = false;
			// then clamp it between whatever and whatever else
			if (returnVal < inMin)
			{
				returnVal = inMin;
				updateSelection = true;
			}

			if (inMax != 0 && returnVal > inMax)
			{
				returnVal = inMax;
				updateSelection = true;
			}

			// finally, actually set the value post clamping
			inBox.Text = returnVal.ToString();

			if (updateSelection)
				inBox.SelectionStart = inBox.Text.Length - 1;

			return returnVal;
			
		}

		// The user is typing. Limit the input, but don't set the final variable yet.
		private void TextBox_Changed( object sender, TextChangedEventArgs e){
			IntegerLimit( txtWrapLength, 100 );
			IntegerLimit( txtBufferSize, 1000 );
		}

		// The user has pushed enter or left the numeric text box, clamp and apply the value
		private void TextBox_KeyUp(object sender, KeyEventArgs e)
		{
			if ( e.Key == Key.Enter ) TextBox_LostFocus( sender, null );			
		}

		// The user has pushed enter or left the text box, clamp and apply the value
		private void TextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			bufferSize = FinaliseTextbox( txtBufferSize, 100, 100000 );
			int oldWrapLength = wrapLength;
			wrapLength = FinaliseTextbox( txtWrapLength, 0, 0 );

			// Update existing items?
			if ( wrapLength != oldWrapLength ){
				
				foreach( LogEntry l in logPraser.pendingLogs ) 
					if ( !string.IsNullOrEmpty( l.unwrapped )  )
						l.text = WrapString( l.unwrapped );
				
				foreach (LogEntry l in listBox1.Items)
					if (!string.IsNullOrEmpty(l.unwrapped) )
						l.text = WrapString(l.unwrapped);
				listBox1.Items.Refresh();	//update our newly-wrapped widths

				foreach (LogEntry l in listBox2.Items)
					if (!string.IsNullOrEmpty(l.unwrapped) )
						l.text = WrapString(l.unwrapped);				
				listBox2.Items.Refresh();

				RefreshViews();

				// Force scroll to current position to bring back into view
				Console.WriteLine( "Re focusing on selection..." );
				Scroll( listBox1, 0 );
				Scroll( listBox2, 0 );

			}

		}

		// Refresh a listview when
		// A: the panel or window has resized
		// B: a new item has been added
		private void RefreshViews(){
			RefreshListView( listBox1, wrapLength == 0 ? ListViewResizeType.TOCONTENT : ListViewResizeType.TOPARENT );
			RefreshListView( listBox2, wrapLength == 0 ? ListViewResizeType.TOCONTENT : ListViewResizeType.TOPARENT );			
		}

		private void RefreshListView( ListView inLV, ListViewResizeType resizeType ){
			
			// Fill the final column up to the size of the ListView control
			if ( resizeType == ListViewResizeType.TOPARENT ){
				
				GridView gv = (GridView)(inLV.View);
				// ty Gary Connell, Konrad Morawski for the scrollbar hint!
				double barWidth = SystemParameters.VerticalScrollBarWidth * 2; // few extra pxs
				double listWidth = Math.Max(0, inLV.ActualWidth - barWidth);

				double columnsWidth = 0;
				for (int i = 0; i < gv.Columns.Count - 1; i++)
				{
					columnsWidth += gv.Columns[i].ActualWidth;
				}

				gv.Columns[gv.Columns.Count - 1].Width = Math.Max(2, listWidth - columnsWidth);

			}

			if ( resizeType == ListViewResizeType.TOCONTENT ){
				
				GridView gv = (GridView)inLV.View;

				// toggle it to an actual value and toggle it back
				// thanks Dr WPF on MSDN for this gem.

				gv.Columns[gv.Columns.Count - 1].Width = gv.Columns[gv.Columns.Count - 1].ActualWidth;
				gv.Columns[gv.Columns.Count - 1].Width = double.NaN;

			}


		}

		private void cbScroll_Checked(object sender, RoutedEventArgs e)
		{
			Console.WriteLine( "Scrolling back to the bottom" );
			Scroll( listBox1, listBox1.Items.Count );
			Scroll( listBox2, listBox2.Items.Count);
		}

		// 110% overkill setting up instanced routed commands, etc for this.
		// Basically, it's this or a separate class with function declarations and relentless busywork
		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			
			MenuItem m = (MenuItem)sender;
			string command = (string)m.Tag;
			ListView source = ((ContextMenu)m.Parent).PlacementTarget as ListView; // lol

			Console.WriteLine( "Command " + command + " from " + source );

			switch( command ){
				
				case "copy": CopyFrom( source ); break;
				case "stack": SearchOnline(source); break;
				case "google": SearchOnline(source, true); break;
				case "save" : SaveContents( source ); break;

			}
			
		}

		public void CopyFrom( ListView whichView ){
			
			string copystring = "";
			for ( int i = 0; i < whichView.SelectedItems.Count; i++ ){
				copystring += whichView.SelectedItems[i] + "\r\n";  //CR/LF for notepad
			}

			Console.WriteLine( "copystring: " + copystring );
			Clipboard.SetData( DataFormats.Text, copystring );

		}

		public void SearchOnline( ListView whichView, bool useGoogle = false ){
			
			string searchString = "";
			for (int i = 0; i < whichView.SelectedItems.Count; i++)
			{
				if ( whichView.SelectedItems[i] is LogEntry ){
					LogEntry l = (LogEntry)whichView.SelectedItems[i];
					if ( !string.IsNullOrEmpty( l.text ) )
						searchString += l.text;
				}
			}
			
			if ( useGoogle ){
				Process.Start("http://www.google.com/search?q=" + System.Uri.EscapeDataString( searchString ) );
			} else {
				Process.Start("http://stackoverflow.com/search?q=" + System.Uri.EscapeDataString(searchString));
			}

		}

		public void SaveContents( ListView inView ){

			// Feel free to add your own flavour of native file dialogue because "Native File Dialog", wxWidgets, etc is out of the question.

			Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
			if (sfd.ShowDialog().GetValueOrDefault())
			{
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < inView.Items.Count; i++)
				{
					sb.Append(((LogEntry)inView.Items[i]).raw);
					sb.Append( "\r\n" );
				}
				System.IO.File.WriteAllText(sfd.FileName, sb.ToString());
			}

		}

		private void Listbox_KeyUp(object sender, KeyEventArgs e)
		{
			
			bool ctrlHeld = (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));

			if ( e.Key == Key.C && ctrlHeld ){
				CopyFrom( (ListView)sender );		
			}

			if (e.Key == Key.G && ctrlHeld)
			{
				SearchOnline( (ListView)sender, true );
			}

			if (e.Key == Key.T && ctrlHeld)
			{
				SearchOnline((ListView)sender, false);
			}

			if ( e.Key == Key.S && ctrlHeld ){
				SaveContents( (ListView)sender );
			}

		}

        private void CbVerbose_Checked(object sender, RoutedEventArgs e)
        {
            levelFilter = (LevelFilterMode.Debug | LevelFilterMode.Info | LevelFilterMode.Warning |
                           LevelFilterMode.Error | LevelFilterMode.Assert);
            OnLogLevelTypeClicked(levelFilter);
        }
        private void CbDebug_Click(object sender, RoutedEventArgs e)
        {
            OnLogLevelTypeClicked(LevelFilterMode.Debug);
        }

        private void CbInfo_Click(object sender, RoutedEventArgs e)
        {
            OnLogLevelTypeClicked(LevelFilterMode.Info);
        }

        private void CbWarning_Click(object sender, RoutedEventArgs e)
        {
            OnLogLevelTypeClicked(LevelFilterMode.Warning);
        }

        private void CbError_Click(object sender, RoutedEventArgs e)
        {
            OnLogLevelTypeClicked(LevelFilterMode.Error);
        }

        private void CbAssert_Click(object sender, RoutedEventArgs e)
        {
            OnLogLevelTypeClicked(LevelFilterMode.Assert);
        }

        private void OnLogLevelTypeClicked(LevelFilterMode mode)
        {
            CheckBox checkbox = null;
            switch (mode)
            {
                case LevelFilterMode.Debug:
                    checkbox = cbDebug;
                    break;
                case LevelFilterMode.Info:
                    checkbox = cbInfo;
                    break;
                case LevelFilterMode.Warning:
                    checkbox = cbWarning;
                    break;
                case LevelFilterMode.Error:
                    checkbox = cbError;
                    break;
                case LevelFilterMode.Assert:
                    checkbox = cbAssert;
                    break;
            }

            bool verbose = (levelFilter == LevelFilterMode.Verbose);

            if (mode != LevelFilterMode.Verbose)
            {
                if (checkbox != null)
                {
                    if (checkbox.IsChecked ?? false)
                    {
                        if (!verbose)
                            levelFilter |= mode;
                        else
                            levelFilter = mode;
                    }
                    else
                    {
                        levelFilter &= ~mode;
                    }
                }
            }

            verbose = (levelFilter == LevelFilterMode.Verbose);
            cbVerbose.IsChecked = verbose;
            cbDebug.IsChecked = !verbose && levelFilter.HasFlag(LevelFilterMode.Debug);
            cbInfo.IsChecked = !verbose && levelFilter.HasFlag(LevelFilterMode.Info);
            cbWarning.IsChecked = !verbose && levelFilter.HasFlag(LevelFilterMode.Warning);
            cbError.IsChecked = !verbose && levelFilter.HasFlag(LevelFilterMode.Error);
            cbAssert.IsChecked = !verbose && levelFilter.HasFlag(LevelFilterMode.Assert);

            UpdateFilters(null, null);
        }

    }


}
