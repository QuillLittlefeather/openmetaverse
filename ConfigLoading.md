# Discussion on config loading mechanism #

## How Apache httpd does it ##

The following is based on what's implemented in Ubuntu's Apache httpd 2.2.x package, though the mechanisms should be similar on other operating systems....

The config files are:

  * `/etc/apache2/apache2.conf`
    * sets some initial, global settings (number of threads, default log locations, etc.)

  * `/etc/apache2/mods-available/*.load`
    * contains all module load (but not configuration!) directives for all installed (but not necessarily used!) modules

  * `/etc/apache2/mods-available/*.conf`
    * contains all module configuration directives for all installed (but not necessarily used!) modules

  * `/etc/apache2/mods-enabled/*.load`
    * symlinks to `mods-available/*.load` files
    * a link here triggers loading of the module
    * loaded in alphanumeric order, so you can play ordering games via:
      * `00-mefirst.load`
      * `05-mesecond.load`
      * etc.

  * `/etc/apache2/mods-enabled/*.conf`
    * same as `mods-enabled/*.load` above, but for the config files

  * `/etc/apache2/httpd.conf`
    * user override settings go here

  * `/etc/apache2/ports.conf`
    * network port settings

  * `/etc/apache2/conf.d/*`
    * random collections of settings, e.g. one by default has a bunch of security-related settings in a file `conf.d/security`

  * `/etc/apache2/sites-available/*`
    * similar to `mods-available`, but for virtual host settings
    * typically one virtual host per file

  * `/etc/apache2/sites-enabled/*`
    * symlinks similar to `mods-enabled`, but for virtual host settings

There are also included some unfortunately-named utilities: `a2enmod`, `a2ensite`, `a2dismod`, `a2dissite`
  * these handle creating and removing the symlinks in mods-enabled and sites-enabled, e.g.:
```
      a2enmod <modulename>
```