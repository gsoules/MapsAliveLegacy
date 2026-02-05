# MapsAlive Legacy Edition installation guide

This guide explains how to install the MapsAlive technology stack components and the MapsAlive
software on a Windows PC. It also explains how to restore your account and tours from
 [mapsalive.com](https://www.mapsalive.com).
 
Read through this entire guide before attempting an installation to be sure
you understand and are comfortable with the process before you begin.

## Overview
Here is an overview of the steps covered in this documentation.

- Install IIS
- Install SQL Server
- Install SQL Server Management Studio
- Download this GitHub distribution to your PC
- Perform IIS and SQL Server configuration
- Restore a MapsAlive database that contains your MapsAlive account
- Set MapsAlive configuration values
- Restore tours from your MapsAlive account
- Troubleshooting

## Install IIS
IIS (Internet Information Services) is Microsoft’s web server platform for hosting websites
and web apps on Windows. It is similar to other web servers like Apache, but is managed using
the Windows IIS program. IIS is required for for ASP.NET web applications like MapsAlive.

This section provides guidance on installing IIS 10, but is not comprehensive.
You must be comfortable installing and configuring this kind of system software.

- Open the Windows Features panel:
    - Open the Start menu and type `Windows Features`
    - Select **Turn Windows features on or off.**
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
    - Run the installer's `.msi` file and follow the wizard steps.
    - Restart IIS by one of these two methods:
        - Run IIS (open the Windows **Start** menu and type `IIS` to find it) and click the **Restart** link, or
        - Open a command prompt as Administrator and run `iisreset`

## Install SQL Server Developer Edition
Microsoft SQL Server Developer Edition is a full-featured, non-production version of SQL Server intended for development and testing, with no feature limits compared to Enterprise. It’s free to use, but is licensed only for building and validating applications—not for running live workloads.

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
        - Choose **Mixed Mode (SQL + Windows)**
    - Add yourself as an admin by clicking **Add Current User**
    - Enable TCP/IP using **Server Configuration Manager**
        -   Open a command window and type `SQLServerManager16.msc`  
         (you might need to change `16` to match your installation)
        - Expand **SQL Server Network Configuration** > **Protocols for MSSQLSERVER**
        - Right-click **TCP/IP** and set it to **Enabled**
        - Double-click **TCP/IP**
        - Go to the **IP Addresses** tab, scroll to the bottom (section **IPAll**), and ensure **TCP Port** is set to `1433`
        - Restart the SQL Server service:
            - Click **SQL Server Services** in the left pane
            - In the right pane, right-click on **SQL Server (MSSQLSERVER)**
            - Choose **Restart** from the menu


## Install SQL Server Management Studio (SSMS)
SQL Server Management Studio (SSMS) is an environment for managing any SQL infrastructure.
It lets you access, configure, manage, administer, and develop all components of a SQL Server Database. Follow these steps to download and install SSMS:

-   Go to the [Microsoft download site for SSMS](https://learn.microsoft.com/en-us/ssms/install/install)
-   Click the button to download the **SQL Server Management Studio 22 installer**
-   Run the installer. It can take several minutes (it seem to hang on step 92 of 200, but eventually completed).
-   Run SSMS. If it prompts you to log in with a Microsoft or GitHub account, click the small "skip" link at the bottom.
-   When logging in you need to check the box for **Trust Server Certificate**

## Download the MapsAliveLegacy GitHub distribution to your PC
Follow these steps to download the distribution.

- Download the files from GitHub
    - Go to the [MapsAliveLegacy main page](https://github.com/gsoules/MapsAliveLegacy)
    - Click **Code** (green button)
    - Choose **Download ZIP**
    - Extract the zip on your PC
- Move the distribution folders to where you want them to reside
    - Do NOT put them inside the `inetpub` folder
    - Choose a folder to use for the website such as `C:\MapsAlive\MapsAliveLegacy`
    - Move the unzipped folder `MapsAliveWebApp` into the website folder
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
- In a later step, you'll copy the tour folders received from AvantLogic into the `Tour` folder    

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
To perform following steps you'll need the `mapsalive_V3.bak` file that AvantLogic gave you.
It contains the production MapsAlive database from `mapsalive.com` but with only your account
and an admin account.

- Run SSMS (SQL Server Management Studio)
- Right click on **Databases** in the Object Explorer
- Choose **Restore Database**
- Choose **Device** as the **Source** and click the ellipsis
- On the **Select Backup Devices** dialog, click the **Add** button
- Navigate to the `.bak` file received from AvantLogic and click **OK**
- Click **OK** on the **Select** dialog
- The restored and destination database names will be `mapsalive_V3`
- On the **Restore Database** dialog click **OK**
- Click **OK** on the success dialog

#### Create SQL Server login for MapsAlive
*Skip this step if you have already created a `mapsalive_V3_login` for SQL Server but you want to restore a new copy of the database.*

A new installation of SQL Server needs a server login for MapsAlive. To create the login:
- Edit the query below to contain a password you like
- Edit the `<connectionStrings>` section of `web.config` to use the same password
- Run the query in SSMS


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
- For the **User** choose `mapsalive_V3_login` and your SQL server password
- Choose `mapsalive_V3` as the database
- Click **Test Connection**

If the connection fails, retrace your steps to see what needs to be corrected.

## Set MapsAlive configuration values

Once the database connection is working, test the installation in a browser by going to `localhost/MapsAlive`. You should see the MapsAlive Legacy Edition page with the following options:
- Login
- Tour Builder
- Create New Account
- Configuration Status

Now you need to configure settings for MapsAlive.

- Login to MapsAlive as `admin@mapsalive` using the password provided by AvantLogic.
- Once you are logged in to the Tour Builder, click **Home** to return to the MapsAlive Legacy Edition page.
- Choose **Configuration Status**.
- Correct the errors shown in red by editing `MapsAlive.config` located in the `MapsAliveWebSite` folder. Below is an example of what the file would look like if the GitHub distribution was copied to `C:\MapsAlive`.

```
<?xml version="1.0"?>
<configuration>
	<runtimeDir value="C:\MapsAlive\MapsAliveWebApp\AppRuntime"/>
	<tourDir value="C:\MapsAlive\MapsAliveWebApp\Tour"/>
	<samplesDir value="C:\MapsAlive\MapsAliveWebApp\Samples"/>
	<siteUrl value="http://localhost/MapsAlive"/>
	<tourUrl value="http://localhost/Tour"/>
	<adminEmail value="support@yourdomain.com"/>
	<logFile value="C:\MapsAlive\MapsAliveWebApp\logs\{0}.log" />
</configuration>
```

- Change the Dir and URL values to match your configuration as set up in IIS.
- If possible, replace `locallhost` with an IP address or domain name to allow MapsAlive to be accessed from other computers.
- Set **adminEmail** to an email address that MapsAlive will use to send emails e.g. to report errors.
- After you edit **logFile**, edit line 176 in `Global.asax` located in the `MapsAliveWebSite` folder so that `logFileNamePattern` uses the same location.
- To apply the edits, click the **Restart** link on the **Configuration Status** page.
- **SMTP** will say "Not Tested" which is okay for now. You don't need SMTP to use the TourBuilder. To configure SMTP, edit the `<smtp>` section in `web.config` located in the `MapsAliveWebSite` folder. You'll need to figure out what the proper settings are for your email server.

## Restore tours from your MapsAlive account

You should now be able to run the MapsAlive TourBuilder and see the tours in your account, but to run the tours you'll need to restore the tour folders. Do this by unzipping the tours zip file received from AvantLogic into your installation's `MapsAliveWebApp\Tour` folder. When you are done, the folder should look something like this:

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
You should now be able to use your local edition of MapsAlive with the tours you previously created on mapsalive.com. You should also be able to create and publish new tours.

#### Creating another MapsAlive account

If you need to create a new MapsAlive account:
- Choose **Create New Account** from the MapsAlive Legacy Edition page to create a trial account
- Log out of the trial account
- Log in as the Admin user
- Choose **Account > Administration > Users** from the Tour Builder menu
- Type the new account's email in the **Search** box and click **Find**
- Click the **Select** link next to the account to get to the **Account Profile** page
- Change **Type** from **Trial** to **Elite**
- You should also change your existing account to **Elite** (Elite accounts never expire)

## Troubleshooting

If you are unsuccessful getting the MapsAlive TourBuilder running, verify the following:
- IIS points to the correct folders.
- The SQL Server installation has a login for `mapsalive_V3` that is mapped to the `mapsalive_V3` database user.
- `web.config` has the correct database server login and password.
- The values in `MapsAlive.config` are correct.
- You have restarted MapsAlive after editing `MapsAlive.config`.
- The MapsAlive Configuration Status page shows `PASS` for **Status**, **Locations**, and **DB Connection**.






