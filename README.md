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

![TopoOASGroupedSelect](https://user-images.githubusercontent.com/65288066/176981885-76a0cb4d-0e3b-4bf1-aa3b-1c76316685ec.png)

The OAS Stages are grouped by Stream.

![TopoOASFilteredSelect](https://user-images.githubusercontent.com/65288066/176981906-d5fe7973-e09c-47a1-b78d-1e4582bf2be8.png)

The select list can be filtered, all stages containing the filter value will be shown.

![TopoOASFilteredMultipleSelect](https://user-images.githubusercontent.com/65288066/176981816-70f8d817-c27d-4064-95d9-c105b6124d84.png)

Multiple stages can be selected at once by holding the ctrl key when clicking on items.

![TopoOAS](https://user-images.githubusercontent.com/65288066/176982005-5676c724-0929-4bee-a2ad-b53a46731e3b.png)

Select the Unit and Stage(s) and click Generate OAS Report to download the report. Use the Hide completed members option to only show in progress members in the report. One page will be created per selected stage. Use the Break by Patrol option to break each stage into one page per patrol.


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

### Wallchart Report
The Wallchart Report provides a report of the Group Life page for a Unit. There is also a csv downlod for Excel.

![TopoWallchart](https://user-images.githubusercontent.com/65288066/168469491-43d9a400-1d6a-4742-b386-310b632a08c3.png)

Select the Unit and click Generate Wallchart report to download the report. The report is designed for an A3 landscape page but will shrink to A4. Click Generate Wallchart csv to download the Excel csv file.

![TopoWallchartReport](https://user-images.githubusercontent.com/65288066/168985163-346e7255-57e1-497b-b587-4a81ae360a77.png)

![TopoWallchartCSV](https://user-images.githubusercontent.com/65288066/168469623-4d5a01c6-92b9-4753-bb18-a6165716cab0.png)

### Additional Awards Report
The Additional Awards Report provides a report of all additional awards awarded to the youth members. Due to the large number of possible additional awards, only those that have been awarded are shown. Also for each member a summary of the KMs hiked and Nights camped is shown against their name to make it easier to determine if a new KMs or Nights badge needs to be given out. There is also an option to create an Excel xlsx download as well.

![TopoAdditionalAwards](https://user-images.githubusercontent.com/65288066/170488868-582eb2c5-14e5-45fc-9ed8-0bfc705d3c78.png)

Select the Unit, select the member(s) or use Select All and click Generate Additional Awards report to download the report.

![TopoAdditionalAwardsReport](https://user-images.githubusercontent.com/65288066/170489141-e2e7fdc5-9ea8-488b-bf8f-14f3cc1096b6.png)

Click Generate Additional Awards xls to download the report in Excel format.

![TopoAdditionalAwardsExcel](https://user-images.githubusercontent.com/65288066/170489286-f8804cc1-d79e-4868-b84d-e711554cb65c.png)

