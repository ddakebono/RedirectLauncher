Redirect Mabinogi Launcher - Mark 2
===================================

Description
-----------

This launcher is built to support most function a Mabinogi private server might want.  
Some parts of this launcher are based off of Logue's MabiLauncher (<a href="https://github.com/logue/MabiLauncher">https://github.com/logue/MabiLauncher</a>)  
For the most part I used Logue's source to figure out how the official stuff worked.  

Features
--------

Right now the launcher is mostly feature complete.  
* Launching the client  
* Self updater for the launcher  
* Client updater that works off the official update servers  
* Reading of a remote patch info file  
* Automatic downloading of a modpack package file for a specific server  
* Launcher web page like the official launcher  
* Custom launcher naming based off the patch info file  

Usage
-----

While the launcher is primarily built to service my own needs I have built it to be easily changed to any other private server.  

This is what the patch info file looks like, note that it still has all the official info with extra stuff tacked on for this launcher.  
A blank copy of this is included in the repo  

> patch_accept=1  
> local_version=205  
> local_ftp=mabipatch.nexon.net  
> main_version=205  
> main_ftp=mabipatch.nexon.net   
> launcherinfo=162  
> redirectlauncherver=11  
> redirectlauncherrepo=http://webhost-url/patchdata/launcher  
> redirectlaunchername=  
> redirect_mod_version=1  
> redirect_mod_repo=http://webhost-url/patchdata  
> redirect_launcher_webpage=http://webhost-url/launcher.html  
> login=(Login server ip)  
> login_port=11000  
> arg=chatip:(Chat server ip) chatport:8002 setting:"file://data/features.xml=Regular, USA" resourcesvr:"http://webhost-url/resources/"  
> lang=patch_langpack.txt  

Along with the patch info file being hosted you need the following directories to make use of the full functions.  
> http://webhost-url/patchdata/launcher - This contains the launcher updates, the launcher expects the following naming on updates launcher-x.exe X being the version. (ex. launcher-11.exe)  
> http://webhost-url/patchdata/package - This contains the modpack package files, the launcher expects the following naming on modpack files modpack-x X being the version (ex. modpack-1)  

Once this file is hosted you just need to change the URL the launcher points at in MainWindow.xaml.cs.  

After the file is hosted and the directories added you can simply launch the launcher with your patch info URL from any folder.  
It will automatically find the client, if it can't it will ask you where it is, if none exists just select any folder and the launcher will download the latest client to C:\Nexon\Mabinogi.  

Misc
----

Like Logue's launcher, this launcher will also start crackshield (HSLaunch.exe) from the client directory should it exist.  
The launcher has only been tested for the North American patch servers and client, code may need tweaking for other regions, fill an issue about it and I'll try to get it working.  

License
-------

MetroRadiance, MetroRadiance.Core and MetroRadiance.Chrome were created by Grabacr07 (<a href="https://github.com/Grabacr07/MetroRadiance">https://github.com/Grabacr07/MetroRadiance</a>)  
MetroRadiance is licensed under the MIT License.

The Launcher also relies on DotNetZip, which I can't find the license for cause the codeplex for it is dead right now.

All of my code in this launcher is licensed under the MIT License  
<a href="http://opensource.org/licenses/MIT">http://opensource.org/licenses/MIT</a>




