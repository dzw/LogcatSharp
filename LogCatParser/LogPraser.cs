using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;

namespace LessShittyLogcat {
    public class LogPraser {
	    // For comparing timestamps / tags and grouping
	    // Can't read out of the "pending" buffer
	    // as that could be interrupted at any point
	    // 2 lines requred as unity sometimes throws in
	    // a blank line
	    LogEntry lastAdded;
	    LogEntry secondLastAdded;

	    // These are thread-specific so let's cache them.
	    // (Used for listview foregrounds)
	    
	    
		// NOT UI THREAD
		public void STDOut_OnDataReceived( object sender, DataReceivedEventArgs e ){
			
			ParseLog( e.Data );

		}
	

		// NOT UI THREAD
		public void ParseLog( string rawString ){
			
			if ( string.IsNullOrEmpty( rawString ) )
				return;

			//Console.WriteLine( "Raw string: " + rawString );
			//11-04 12:56:54.501  4483  5460 I AlarmManager: setExactAndAllowWhileIdle [name: GCM_HB_ALARM type: 2 triggerAtMillis: 5557789]

			int timeStart = 0;
			int timeSplit = rawString.IndexOf('.', 0) + 4;		//11-04 12:55:50.857
			if ( timeSplit == -1 ) goto LBL_CantParse;
			string timeString = rawString.Substring(timeStart, timeSplit);

			int pidStart = timeSplit + 3;			
			int pidSplit = rawString.IndexOf(' ', pidStart + 1);
			if ( pidSplit == -1 ) goto LBL_CantParse;
			string pidString = rawString.Substring(pidStart, pidSplit - pidStart);

			int tidStart = pidSplit + 3;			
			int tidSplit = rawString.IndexOf(' ', tidStart + 1);
			if ( tidSplit == -1 ) goto LBL_CantParse;
			string tidString = rawString.Substring(tidStart, tidSplit - tidStart);

			int tagStart = tidSplit + 3;			
			int tagSplit = rawString.IndexOf( ':', tagStart + 1 );
			if ( tagSplit == -1 ) goto LBL_CantParse;
			string tagString = rawString.Substring(tagStart, (tagSplit - tagStart) + 1);

			int levelStart = tidSplit + 1;
			string levelString = rawString.Substring( levelStart, 1 );


			bool sameTimestamp = false;

			// Same timestamp as previous means it's a multipart log entry
			// (Unity) So indent it.
			if ( lastAdded != null ){				
				if ( timeString == lastAdded.time 
					&& tagString == lastAdded.tag 
					&& levelString == lastAdded.level					
				){					
					sameTimestamp = true;
				}
			}

			// Exception: E.g. if 2x unity multipart messages arrive with the same timestamp
			// this unglues them
			if ( secondLastAdded != null ){
				if ( secondLastAdded.tag.IndexOf( "Unity" ) >= 0 ){					
					if ( secondLastAdded.text.IndexOf( MULTI_SEPARATOR + "(Filename" ) >= 0 )
						sameTimestamp = false;
				}
			}

			int textStart = tagSplit + 2;			
			int textSplit = rawString.Length;
			if ( textSplit == -1 ) goto LBL_CantParse;
			string textString =  (sameTimestamp ? MULTI_SEPARATOR : "" ) +  rawString.Substring(textStart, textSplit - textStart);
			
			LogEntry l = new LogEntry()
			{
				level = levelString,
				time = timeString,
				PID = pidString,
				TID = tidString,
				app = tagString, // not implemented
				tag = tagString,
				unwrapped = textString,
				text = /*WrapString*/( textString ),
				raw = rawString				
			};

			switch( levelString ){				
				case "I" : l.color = Colors.Black; break;
				case "W": l.color = Colors.DarkOrange; break;
				case "E": l.color = Colors.DarkRed; break;
				default: l.color = Colors.DarkBlue; break;
			}

			secondLastAdded = lastAdded;
			lastAdded = l;
			pendingLogs.Add(l);

			return;

			LBL_CantParse:

			LogEntry f = new LogEntry(){ text = rawString, color = Colors.DarkBlue };
			pendingLogs.Add( f );

		}
		const string MULTI_SEPARATOR = " || ";
		
		// Storage between the event thread and UI thread
		// (this is the list that fills while we're paused)
		public List<LogEntry> pendingLogs = new List<LogEntry>();
    }
}