# MapsAlive Legacy Edition installation guide

This guide explains how to install the MapsAlive technology stack components and the MapsAlive
software on a Windows PC. It also explains how to restore your account and tours from
 [mapsalive.com](https://www.mapsalive.com).
 
Read through this entire guide before attempting an installation to be sure
you understand and are comfortable with the process before you begin.

## Overview
Here is an overview the steps you will need to perform.

- Install IIS
- Install SQL Server
- Install SQL Server Management Studio
- Download this GitHub distribution to your PC
- Perform IIS and SQL Server configuration
- Restore a MapsAlive database that contains your MapsAlive account
- Set MapsAlive configuration values
- Restore tours from your MapsAlive account

## Install IIS
IIS (Internet Information Services) is Microsoft’s web server platform for hosting websites
and web apps on Windows. It is similar to other web servers like Apache, but is managed using
the Windows IIS program. IIS is required for for ASP.NET web applications like MapsAlive.

This section provides guidance on installing IIS 10, but is not comprehensive.
You must be comfortable installing and configuring this kind of system software.

- Open the Windows Features panel:
    - Open the Start menu and type `Windows Features`
    - Select **“Turn Windows features on or off.”**
- Enable the following IIS components:
    - **.NET Framework 3.5 (includes .NET 2.0 and 3.0)**
    - **Web Management Tools**
        - **IIS Management Console**
    - **World Wide Web Services**
        - **Application Development Features**
            - **.NET Extensibility 3.5**
            - **ASP.NET 3.5**
            - **ISAPI Extensions**
        - **ISAPI Filters**
            
    - **Common HTTP Features**
        - **Default Document**
        - **Static Content**
        - **Directory Browsing**
        - **HTTP Errors**
        - **HTTP Redirection**
            
    - **Security**
        - **Request Filtering**
        - **Windows Authentication**
- Confirm that IIS is installed
    - Open a browser window and go to `http://localhost`
    - You should see the default IIS welcome page
- Install the IIS URL Rewrite component
    - Go to [https://www.iis.net/downloads/microsoft/url-rewrite](https://www.iis.net/downloads/microsoft/url-rewrite)
    - Scroll to the bottom of that page and choose the **x64** installer
    - Run the `.msi` and complete the wizard.
    - Restart IIS by one of these two methods:
        - Run IIS (open the Start menu and type `IIS` to find it) and click the restart link
        - Open a command prompt as Administrator and run `iisreset`

## Install SQL Server Developer Edition
Microsoft SQL Server Developer Edition is a full-featured, non-production version of SQL Server intended for development and testing, with no feature limits compared to Enterprise. It’s free to use, but licensed only for building and validating applications—not for running live workloads.

- Download SQL Server Developer Edition
    - Go to the [Microsoft SQL Server downloads page](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
    - Select **SQL Server 2025 Developer** (or the latest Developer edition available)
    - Download the installer for the Standard Developer Edition (`SQL2025-SSEIDev.exe`)
    - Run the installer and choose a custom installation
    - Keep the default instance name `MSSQLSERVER`
    - There are too many options to cover here, but choose:
        - **Database Engine Services** (core SQL Server engine)
        - **Full‑Text and Semantic Search**
        - **SQL Server Replication**
    - You *don't* need:
        - **Analysis Services**
        - **Reporting Services**
        - **Machine Learning Services**
    - Configure server authenticaion
        - Choose **Mixed Mode (SQL + WIndows)**
    - Add yourself as an admin by clicking **Add Current User**
    - Enable TCP/IP
        - Open SQL Server Configuration Manager
        -   Open a command window and type `SQLServerManager16.msc`
        - Expand **SQL Server Network Configuration** > **Protocols for MSSQLSERVER**
        - Right-click **TCP/IP** and set it to **Enabled**
        - Double-click **TCP/IP**
        - Go to the **IP Addresses** tab, scroll to the bottom (**IPAll**), and ensure **TCP Port** is set to `1433`
        - **Restart** the SQL Server service in the "Services" tab of the Configuration Manager.

## Install SQL Server Management Studio (SSMS)
SQL Server Management Studio (SSMS) is an environment for managing any SQL infrastructure.
It lets you access, configure, manage, administer, and develop all components of your Database.

- Download and install SSMS
-   Go to the [Microsoft download site for SSMS](https://learn.microsoft.com/en-us/ssms/install/install)
-   Click the button to download the **SQL Server Management Studio 22 installer**
-   Run the installer. It can take several minutes (it seem to hang on step 92 of 200, but eventually completed).
-   Run SSMS. If it prompts you to log in with a Microsoft or GitHub account, click the small "skip" link at the bottom.
-   When logging in you need to check the box for **Trust Server Certificate**

## Download this GitHub distribution to your PC
Here the steps to download the distribution.

- Download the files from GitHub
    - Go to the [MapsAliveLegacy main page](https://github.com/gsoules/MapsAliveLegacy)
    - Click ****Code** (green button)
    - Choose **Download ZIP**
    - Extract the zip on your PC
- Move the distribution folders to where you want them to reside
    - Do not put them inside the `inetpub` folder
    - Choose a folder to use for the web site such as `C:\MapsAlive\MapsAliveLegacy`
    - Move the unzipped folder `MapsAliveWebApp` into the web site folder
    - You should now have a structure like this:
    ```
    C:\MapsAlive\MapsAliveLegacy\MapsAliveWebApp
        AppRuntime
        MapsAliveWebSite
        Tour
    ```
- Create these two empty folders within the `MapsAliveWebApp` folder:
    - `logs`
    - `Samples` (not used but needed for configuration purposes)
- Inside the `MapsAliveWebApp\MapsAliveWebSite` folder, create an empty `Bin` folder
    - You should now have a structure like this:
    ```
    C:\MapsAlive\MapsAliveLegacy\MapsAliveWebApp
        AppRuntime
        logs
        MapsAliveWebSite
            Admin
            App_Code
            ...
            Bin
            Controls
            ...
            XSL
        Samples
        Tour
    ```
- Copy the DLLs received from AvantLogic into the `Bin` folder
- Copy the tour folders received from AvantLogic into the `Tour` folder    

## Perform IIS and SQL Server configuration
The following steps explain how to tell IIS where your MapsAlive website is located.

- Run **Internet Information Services (IIS) Manager**
- Click on **Application Pools** and select **DefaultAppPool**
    - Set **.NET CLR version** to **v2.0**
    - Set **Managed Pipeline Mode** to **Integrated**
- Right-click on **Sites > Default Web Site** and choose **Add Application**
	- Set **Alias** to `MapsAlive`
    - Set **Application pool** to `DefaultAppPool`
	- Set **Physical path** to the location of the `MapsAliveWebApp\MapsAliveWebSite` folder
- Right-click on **Default Web Site** and choose Add Virtual Directory
	- Set **Alias** to `Tour`
	- Set **Physical path** to the location of the `MapsAliveWebApp\Tour` folder
- Right-click on **MapsAlive** and choose Add Virtual Directory
	- Set **Alias** to to `AppRuntime`
	- Set **Physical path** to to the location of the `MapsAliveWebApp\AppRuntime` folder

At this stage you should be able to open a browser and go to `localhost/MapsAlive` to verify
that IIS is configured properly. You should see the **Connection Dropped** page which indicates
that the web app is running, but can't communicate with the database. If you don't get to that
page you'll need to retrace your steps to determine what needs to be corrected.

## Restore a MapsAlive database that contains your MapsAlive account
To perform following steps you'll need the `mapsalive_v3.bak` file that AvantLogic gave you.
It contains the production MapsAlive database from `mapsalive.com` but with only your account
and an admin account.

- Run SSMS (SQL Server Management Studio)
- Right click on **Databases** in the Object Explorer
- Choose **Restore Database**
- Choose **Device** as the **Source** and click the ellipsis
- On the **Select Backup Devices** dialog, click the **Add** button
- Navigate to the `.bak` file received from AvantLogic and click **OK**
- Click **OK** on the **Select** dialog
- The restored and destination database names will be `mapsalive_v3`
- On the **Restore Database** dialog click **OK**
- Click **OK** on the success dialog

#### Create SQL Server login for MapsAlive
*Skip this step if you have already created a `mapsalive_V3_login` for SQL Server*

A new installation of SQL Server needs a server login for MapsAlive. To create the login, run the query below in SSMS. Choose any password you like, but you must use the same password in the `<connectionStrings>` section of web.config. 

```
-- Create a login that lets MapsAlive connect to SQL Server
USE [master];
GO
CREATE LOGIN [mapsalive_V3_login] 
WITH PASSWORD = 'm@p$alive', 
CHECK_POLICY = OFF; -- Turn off policy so it doesn't force a password change
GO
```
 
#### Map SQL Server login to database user
 You only need to run the above query once, but you need to run the following queries each time after you restore the MapsAlive database. That's because the database user (the permissions inside the DB) is restored along with the DB, but not the connection to the server login. To connect the server and database logins, run the following query in SSMS:

```
-- Map the V3 database user to the server login (there is no database login)
USE [mapsalive_V3];
GO
ALTER USER [mapsalive_V3_login] WITH LOGIN = [mapsalive_V3_login];
GO

-- Give the database user owner permissions
ALTER ROLE [db_owner] ADD MEMBER [mapsalive_V3_login];
GO
```

You can test the database connection this way:
- Create a blank file on your desktop named `test.udl`
- Double-click it
- Under the **Provider** tab, select **Microsoft OLE DB Provider for SQL Server**
- Under **Connection**, enter `localhost`
- For the **User** choose `mapsalive_v3_login` and your SQL server password.
- Choose `mapsalive_v3` as the database.
- Click **Test Connection**.

## Set MapsAlive configuration values

Once the database connection is working, test that the MapsAlive Legacy Edition page comes up
in a browser by going to `localhost/MapsAlive`. You should see the MapsAlive Legacy Edition page.

Now you need to configure settings for MapsAlive.

- Login to MapsAlive as `admin@mapsalive` using the password provided by AvantLogic
- On the MapsAlive Legacy Edition page choose **Configuration Status**
- Correct the errors shown in red by editing `MapsAlive.config` located in the `MapsAliveWebSite` folder
- Below is an example of what the file would look like if the GitHub distribution was copied to `C:\AvantLogic`

```
<?xml version="1.0"?>
<configuration>
	<runtimeDir value="C:\AvantLogic\MapsAliveWebApp\AppRuntime"/>
	<tourDir value="C:\AvantLogic\MapsAliveWebApp\Tour"/>
	<samplesDir value="C:\AvantLogic\MapsAliveWebApp\Samples"/>
	<siteUrl value="http://localhost/MapsAlive"/>
	<tourUrl value="http://localhost/Tour"/>
	<adminEmail value="support@yourdomain.com"/>
	<logFile value="C:/AvantLogic\MapsAliveWebApp\logs\{0}.log" />
</configuration>
```

- Change the Dir and URL values to match your configuration as set up in IIS.
- If possible, replace `locallhost` with an IP address or domain name to allow MapsAlive to be accessed from other computers.
- Set **adminEmail** to an email address that MapsAlive will use to send emails e.g. to report errors.
- After you edit **logFile**, edit line 176 in `Global.asax` located in the `MapsAliveWebSite` folder to use the same location.
- To apply the edits, click the **Restart** link on the **Configuration Status** page.
- **SMPT** will say "Not Tested" and will change to an error if you click the **Send test email** link.
- To configure SMPT, edit the `<smtp>` section in `web.config` located in the `MapsAliveWebSite` folder.

## Restore tours from your MapsAlive account

You should now be able to run the MapsAlive TourBuilder and see the tours in your account, but to run the tours you'll need to restore the tour folders. Do this by unzipping the tours zip file received from AvantLogic into your installation's `MapsAliveWebApp\Tour` folder. The folder should look something like this:

```
43204
43204_
...
85892
85892_
85893
Default.aspx
web.config
```
You should now be able to use your local edition of MapsAlive with the tours your previously created on mapsalive.com and you should be able to create and publish new tours.






