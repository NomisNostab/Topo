To install.
Create a folder in your Documents folder called Topo.
Copy the contents of Topo folder to the Documents/Topo folder.

To run the app server, double click of the Topo.exe file, or right click and Run as Administrator.

This will open a terminal window showing
	info: Microsoft.Hosting.Lifetime[14]
		  Now listening on: http://localhost:5000
	info: Microsoft.Hosting.Lifetime[0]
		  Application started. Press Ctrl+C to shut down.
	info: Microsoft.Hosting.Lifetime[0]
		  Hosting environment: Production
	info: Microsoft.Hosting.Lifetime[0]
		  Content root path: C:\Users\simon\source\repos\Topo\Topo\bin\Release\net6.0\publish\
or similar.	  

To log into the Reporting application, ctrl-click on http://localhost:5000 to open in your browser.
You could also open a browser and put http://localhost:5000 into the address bar.
This should present you with a page with a Login button. Click the button and enter your Terrain credentials.

When you are finished, close the browser window, go back to the terminal window and close it.

The download and code can found at
https://github.com/NomisNostab/Topo/releases

Troubleshooting

To get the log files to help in fixing a crash:
1. Stop and start Topo.
2. Do whatever you did to make Topo crash
3. Stop Topo
4. In the Topo folder find the file nlog-Topo-all.log and email it to simon.batson@nsw.scouts.com.au
