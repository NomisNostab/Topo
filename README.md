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
Select your unit and the date to search, click Show Unit Calendar. This will show the unit events one month either side of the selected date.

![TopoProgramEventList](https://user-images.githubusercontent.com/65288066/161260135-d7e3ded6-1877-45f7-a88c-d3a14b73699a.png)

The Generate Sign In Sheet link, to the right of the event, will download a sign in sheet pdf document to facilitate recording attendance, leaders are included.

The Download Attendance link will download an attendance list in csv format, if such a report is required.

### OAS Reports
The OAS Report shows for a given unit, stream and stage what each I Statement a youth member has completed and when.

This can be handy when planning events around OAS streams.

![TopoOAS](https://user-images.githubusercontent.com/65288066/161261366-2f0a592d-75c0-44b9-a343-7d18d59eddec.png)

Select the Unit, Stream, Stage and click Generate OAS Report to download the report.

![TopoOASReport](https://user-images.githubusercontent.com/65288066/161261855-99dc0f97-115c-45ee-8564-f03f7dbe4451.png)

### SIA Report
The SIA Report shows for the selected Unit the status of each SIA project undertaken by all the members of the Unit.

![TopoSIA](https://user-images.githubusercontent.com/65288066/162437825-e990677d-714e-4260-8a60-45dd479f2f9c.png)

Select the Unit and click Generate SIA Report to download the report.

![TopoSiaReport](https://user-images.githubusercontent.com/65288066/162438257-c96780b4-5dc4-4f10-9e63-03725d4ae40b.png)
