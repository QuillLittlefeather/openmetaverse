# Installation #

## A word about dependencies ##
Make sure you have the all of the prerequisites installed. These will vary based on Operating System and Operating System version. Only a few linux distributions have seen deployments; they are listed below with the commands to install their dependencies:

> _Debian/Ubuntu_:
> > `apt-get install apache2 php5 php5-mysql php5-curl php5-gd php5-xmlrpc mysql-server`


> _OpenSUSE_:
> > `zypper install apache2 php5 php5-mysql php5-curl php5-gd php5-xmlrpc php5-openssl mysql`

Note: fairly recent linux distros are required, or must otherwise be made to supply php 5.3 and mysql 5.1

Windows has seen successful deployments using both XAMP and manually installed apache2/php5/mysql.


## MySQL Tuning ##

You will want to add the following to your MySQL's 'My.cnf'. If the section `[mysqld]` already exists, just add the max\_allowed\_packet directive to it:

`[mysqld]`

> `max_allowed_packet=16M`

This is needful as the default value of 1MB is too small to load the default assets.


# Services Overview #

Simian Grid Services currently come in a set of three. These are GridFrontend, Grid, and GridWebDAV. These services provide a grid user account management interface (including a user/grid management interface for those with suitable access), a Grid backend, which provides various services that comprise a grid in the OpenSimulator technology paradigm, including inventory, assets, user accounts, region registration, and others; and finally some limited WebDAV inventory management functionality.


# Installation Overview #

In the course of installation, you will address the following basic concerns to get Simian Grid Services up and running on an apache web server.

> o Getting the Simian Grid Services

> o Unpacking Simian Grid Services

> o Establishing Database and Database Credentials

> o Setting Permissions

> o Running the installer

> o Testing/Using Simian Grid Services


## Getting the Simian Grid Services ##

How you get Simian Grid Services will depend on who you are. If you are obtaining it as an end user, you will want to obtain the distribution release package from the OpenMetaverse Foundation website. If you are a  developer, you will need to prepare for git repository use (if you haven't already), and obtain the code from the OpenMetaverse Foundation GitHub site.

The distribution release package can be found at:
```
http://wheresmyfiles.com/download/me/some/simiangrid.php
```

and the source code repository is at:
```
git clone http://github.com/openmetaversefoundation/simiangrid.git
```


## Unpacking Simian Grid Services ##

If you are working as a developer (i.e., with a git repository), you will need to enter your repository and make an archive of your branch. Consult the git documentation for how to accomplish this, if you don't already know. The output of this process will produce a file not unlike the distribution, which may be unpacked in the same manner as described subsequently for the end user.

It is assumed that you have sufficient knowledge to use archive tools on your chosen platform.

Where you unpack the archive is of critical importance. It must be in a web server's document tree in a path on the tree over which you have administrative control. This can be in your public\_html folder or in the server's primary document path, but you must have the ability to create files and allocate permissions. It is of considerable importance that you be able to specify this path accurately in future steps. These instructions will assume you have root access to an apache server that lives in /var/www/ on a linux box, and that you will install into the Document Root (/var/www/htdocs/).

Unpack the archive into /var/www/htdocs/. It will produce a number of files and directories there.

Having successfully unpacked the archive, you are now ready to change the ownership and allocate permissions. If you are installing on windows, you probably wont have to perform this step, but if you do, the key is to give the web server the permissions it needs to write files in the document root. This permission can be revoked once the Simian Grid Services are configured.

Each service uses the same installer code. Installing each is just a matter of opening the correct URL for each service in a browser. But before we do that we must prepare the database and set the permissions on the server paths.


## Establishing Database and Database Credentials ##

Before running the installer, the database environment must be prepared. This is a relatively minor database administration task, and most opensim operators are probably familiar with the operation. If you are not, you should be. I will cover the high points, but will not get too deeply into it - rather, I would refer you to the MySQL documentation at http://dev.mysql.com

There are two minimum requirements:

o a MySQL database user and password

o a database that user can access

In this minimalist scenario a single database will hold a common tablespace for all of the services we install. However, one might break the tablespace logically into seperate databases, i.e., one each for each of the services; additionally, one could have a user per-service as well.

If you do not already have such a database user available, you will need to create one. Being rather comfortable with the command line, I do a lot of work that way, and so I typically do this by logging in to the MySQL instance with the command line MySQL tool, as in 'mysql -u root -p'. This essentially means start the client as the root mysql user and prompt for root's password.

Having logged in, I then create the database that will be used for the Simian Grid Services by executing the SQL '`create database Simian;`'. As noted above, this database will provide a container for the Simian Grid Services' tables.

Next, I create the user with a grant statement as follows:

`grant all on Simian.* to 'sql_user'@'%' identified by 'sql_user_password';`

This gives us the three MySQL parameters we need to complete the installation:

o The database

o The user

o The password


## Setting Permissions ##

> execute '`chown -R www-data:www-data /var/www/htdocs/*`' at a root shell prompt.

The user and group may differ on your linux distribution, but as noted for windows users, the key is to make the document root writable by the web server. This is so that the installer script can write the operational parameters collected by the installer interface into your service configurations. Once installation has been completed, standard operating procedures/best practices can be employed to lock down the system as you would any web service environment.

Depending on the default permissions provided on your system, you may or may not have to adjust permissions on the installer path. If you should have to do so, this is done with the chmod command as follows:

`chmod -R 0775 /var/www/htdocs`

This will give the webserver the ability to read/write/create/delete anything in it's Document Root; not something one would want to leave as-is in a production environment. Again, once installation has been completed, standard operating procedures/best practices can be employed to lock down the system as you would any web service environment.


## Running the Installer ##

The installer is launched by visiting a URL with a browser and filling configuration data into a sequence of forms that are presented. It is run once in turn for each service that will be configured. In spite of the generic approach, Simian Grid Services are a bit codependent; at the very least you will need to install first the Grid Service followed by the GridFrontend. GridFrontend is required to load a grid's starter assets. We will cover all three Simian Grid services at a single stroke now.

There are three installer URLs:

http://myserver.com/Grid/install.php

http://myserver.com/GridFrontend/install.php

http://myserver.com/GridWebDAV/install.php


They should be visited in roughly that order. Note that GridWebDAV will not be useful until your grid has users with inventories.

These forms will prompt you for all the information generated or recorded in the course of the installation process to this point. Work through each of the simple forms that are presented, supplying this information. Pay particular attention to the base URL.

When you have successfully completed the installation, a `'lockfile'` will be created to let the installer know that it has been successfully run. Should you need to run the installer again, you will need to remove it first. It can be found in the root directory of the service; e.g., /var/www/htdocs/Grid/. You can safely remove this file and  run the installer again to update the configuration at any time.

Note that the installer code automatically updates database schemae on the fly, so is entirely appropriate for purposes of upgrading a previous Simian Grid Services installation.

When successful installation has been completed, your Simian Grid Services should be ready to test and use.

## Testing/Using Simian Grid Services ##

Visit the URL that SimianGrid is installed at. In our example work, it would look something like http://myserver.com/Grid/ -- if everything is working correctly it should print out `SimianGrid`. If you receive a blank page or an error, check for a log file in the `logs/` directory or your Apache error log.

Access the Grid Frontend next. It's URL, again according to our example work, would be http://myserver.com/GridFrontend/ -- and if all went well, you should see and OpenMetaverse Foundation-branded dynamic website which will provide you and your grid users ready access to account management and other web-based grid operations infrastructure.

Similarly, GridWebDAV would be accessed via http://myserver.com/GridWebDAV/

note that in the current release, WebDAV support is read-only.


> ### Getting a Simulator Running ###
For the present, we will only examine connecting a core opensim region to Simian Grid Services. This is accomplished identically to connecting a core opensim instance to a Robust Grid Services stack, with the exception that the architecture 'Include'ed at the end of `bin/OpenSim.ini` is different: it should include `config-includes/SimianGrid.ini' instead of `config-includes/Grid.ini`, and the format of the URI will be slightly different.

Once you have configured your `bin/OpenSim.ini` file to use the `config-includes/SimianGrid.ini` include, and modified your `GridCommon.ini` file with the Simian Grid Service URIs (look at `config-includes/SimianGrid.ini` for example URIs), you should be able to start one or more core opensim regions that will register themselves with the grid service.

It should now be possible to login with an opensim-compatible viewer. If you installed Simian Grid Services at `http://www.myserver.com/`, launch your viewer with:

```
-loginuri http://www.myserver.com/login/
```

> (note the trailing slash). You may need to append index.php to the end of the loginuri if your web server is not configured to serve up php files by default.