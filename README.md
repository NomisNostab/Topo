# Topo
Topo is a helper application to assist scout leaders in getting meaningful reports out of the Scouts Australia Terrain applicatiom.
## Installation
### Windows
Download the latest Topo-win.zip file from the [releases](https://github.com/NomisNostab/Topo/releases) page.

Create a folder in your Documents folder called Topo.

Unzip the Topo-win.zip file to the Documents/Topo folder.
### OSX
Download the latest Topo-osx.zip file from the [releases](https://github.com/NomisNostab/Topo/releases) page.

Unzip to your desired location

## Running the Server App
To run the app server, double click the Topo.exe file.
This will open a terminal window similar to

![TopoServerApp](https://user-images.githubusercontent.com/65288066/161207943-1112f345-5fa0-4029-ae4b-06ca799ede5d.png)

When you are finished you can either type Ctrl+c in the window or close the window.

If you get an error when using the reporting app, run the app server by right clicking on the Topo.exe file and select Run as Administrator.

## Using the Reporting App
To access the Reporting application, ctrl-click on http://localhost:5000 to open in your browser.
You could also open a browser and put http://localhost:5000 into the address bar.

![TopoHomeNotLoggedIn](https://user-images.githubusercontent.com/65288066/161263668-009cfa43-5929-4a9b-b317-aab2ae740a94.png)


### Logging In
Click Login, enter your current Terrain credentials and click Sign in

![TopoSignIn](https://user-images.githubusercontent.com/65288066/161263902-607a1a9a-8ae4-448f-825b-10c465b885b2.png)

The home page now shows your available options.

![TopoHomeLoggedIn](https://user-images.githubusercontent.com/65288066/162437205-67e9d7fa-700b-4623-8c61-f7d6772ae711.png)
### Member Lists
Select your unit and the list of members for that unit will be shown.

![TopoMemberList](https://user-images.githubusercontent.com/65288066/161264396-ff0a43c8-530b-4e05-8adc-a022693e19a4.png)

At the bottom of the list are buttons to print the patrol list and the patrol sheets.
The patrol sheets list each patrol on a single A4 to be attached to the patrol corners.

![TopoMemberListButtons](https://user-images.githubusercontent.com/65288066/161210350-28a24a28-0cce-44ab-8009-365919552627.png)

These will download as pdf documents.

### Program Events
Select your unit and the date range to search, click Show Unit Calendar. This will show the unit events for the selected date range.

![TopoProgramEventList](https://user-images.githubusercontent.com/65288066/167407623-33186a83-fd10-4baf-9883-3926be2669b7.png)

The Generate Sign In Sheet link, to the right of the event, will download a sign in sheet pdf document to facilitate recording attendance, leaders are included.

The Download Attendance link will download an attendance list in csv format, if such a report is required.

The Generate Attendance report button will generate and download the report.

It shows all events between two dates with a breakdown of the number of events in each challenge area. The events are colour coded as well.
Each row shows the attendance of the youth and adult with a total at the end. Appended to the name is a percentage attendance rate based on attendance from the starting date to the ending date (or the current date if the ending date is in the future).

The Generate Attendance csv button will generate and download the same data in a csv file.

![TopoAttendanceReport](https://user-images.githubusercontent.com/65288066/167408457-f65cb55c-f103-4d34-8e36-f2248a01962f.png)

### OAS Reports
The OAS Report shows for a given unit, stream and stage what each I Statement a youth member has completed and when.

This can be handy when planning events around OAS streams.

![TopoOAS](https://user-images.githubusercontent.com/65288066/166455113-1829a455-a9eb-4c7c-ab29-4b92a0e9b4b4.png)

Select the Unit, Stream, Stage and click Generate OAS Report to download the report. Use the Hide completed members option to only show in progress members in the report.

![TopoOASReport](https://user-images.githubusercontent.com/65288066/161261855-99dc0f97-115c-45ee-8564-f03f7dbe4451.png)

### SIA Report
The SIA Report shows for the selected Unit and members the status of each SIA project undertaken by all the members of the Unit.

![TopoSIA](https://user-images.githubusercontent.com/65288066/166454635-69d03784-c3dc-47b9-92e1-10475a9db995.png)

Select the Unit, select the member(s) or use Select All and click Generate SIA Report to download the report.

![TopoSiaReport](https://user-images.githubusercontent.com/65288066/162438257-c96780b4-5dc4-4f10-9e63-03725d4ae40b.png)

### Milestone Report
The Milestone Report shows for the selected Unit the current milestone progress for all members of the Unit.

![TopoMilestone](https://user-images.githubusercontent.com/65288066/163992833-2a30689e-7138-4f96-9b5b-1c2bbe024fbf.png)

Select the Unit and click Generate Milestone Report to download the report.

![TopoMilestoneReport](https://user-images.githubusercontent.com/65288066/166454006-7c9a58e7-37d4-4497-9ed6-8fd1b2cb432c.png)


### Logbook Report
The Logbook Report shows for the selected Unit and members the complete logbook history and a total for the KMs hiked and nights camped.

![TopoLogbook](https://user-images.githubusercontent.com/65288066/165905076-79723a0c-f139-4b44-b549-8e7873f789dd.png)

Select the Unit, select the member(s) or use Select All and click Generate Logbook report to download the report.

![TopoLogbookReport](https://user-images.githubusercontent.com/65288066/165905298-0b00ed9e-c36d-4590-994b-ffff7e1b04ed.png)
