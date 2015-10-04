# Installation #

Until SimianGrid has its first versioned released, you will need to check the code out of the git source repository. Use this command to check out the project source code into a folder named `simiangrid`:

```
git clone http://github.com/openmetaversefoundation/simiangrid.git
```

Place this, or a copy of it, somewhere accessible to your webserver.

`SimianGrid` contains three projects, `Grid` (SimianGrid), `GridFrontend` (SimianGridFrontend), and `GridWebDAV` (SimianGridWebDAV). They have only been tested on apache2 and php5.3.

## MySQL ##

You'll need to set `max_allowed_packet` in your my.cnf; the default value of 1MB is too small to load the default assets.  So set:

> `max_allowed_packet=16M`

in the `[mysqld]` section of my.cnf.


## SimianGrid ##

SimianGrid is the core set of web services that provide a persistence layer for your world.

  1. Make sure you have the all of the prerequisites installed.  Some examples which will install everything you need:

> _Debian/Ubuntu_:
> > `apt-get install apache2 php5 php5-mysql php5-curl php5-gd php5-xmlrpc mysql-server`


> _OpenSUSE_:
> > `zypper install apache2 php5 php5-mysql php5-curl php5-gd php5-xmlrpc php5-openssl mysql`


Note: fairly recent linux distros are required, or must otherwise be made to supply php 5.3 and mysql 5.1


  1. Visit Grid/install.php (relative to where you copied the Simian directory previously) and follow through the installation steps. This helper will create config files, load database schemas and fixtures and check for file and directory permissions where required.
# After running the installer, you may wish to remove write permissions from the config files.
  1. Visit the URL that SimianGrid is installed at. If everything is working correctly it should print out `SimianGrid`. If you receive a blank page or an error, check for a log file in the `logs/` directory or your Apache error log.

## SimianGridFrontend ##

SimianGridFrontend is a user-facing web portal for your world that provides user creation and account management, grid administration, and web-based world login. Currently, SimianGridFrontend is required to import the default asset set into SimianGrid.

  1. Visit GridFrontend/install.php (relative to where you copied the Simian directory previously) and follow through the installation steps. This helper will create config files, load database schemas and fixtures and check for file and directory permissions where required.
# After running the installer, you may wish to remove write permissions from the config files.
    1. Go to the starting page for SimianGridFrontend and verify that it works. Next, try creating a user account and logging in and logging out.


## Running with OpenSim ##

  1. Once you have configured your OpenSim.ini file to use the SimianGrid.ini include, and modified your GridCommon.ini file with the SimianGrid service URLs (look at SimianGrid.ini for example URLs), you should be able to start one or more OpenSim regions that will register themselves with the grid service.
  1. You can now login with an OpenSim-compatible viewer. If you installed SimianGrid at `http://www.mygrid.com/`, launch your viewer with:

```
-loginuri http://www.mygrid.com/login/
```


> (note the trailing slash). You may need to append index.php to the end of the loginuri if your web server is not configured to serve up php files by default.


## Troubleshooting ##

Problem: Web server logs report "PHP Fatal error:  Call to undefined function openssl\_random\_pseudo\_bytes()".

Solution: Verify you have PHP >= 5.3 installed.  Verify that the OpenSSL extension is enabled for PHP (e.g. `extension=openssl.so` in php.ini), even if you're not using HTTP over SSL/TLS.