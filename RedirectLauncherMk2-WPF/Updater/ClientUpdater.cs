/*
* Copyright 2015 Owen Bennett
* This file is a part of the RedirectLauncher by Owen Bennett.
* All code contained in here is licensed under the MIT license.
* Please fill issue report on https://github.com/ripxfrostbite/RedirectLauncher
*/
using Ionic.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RedirectLauncherMk2_WPF
{
	class ClientUpdater
	{
		private Game client;
		private DirectoryInfo updateDirectory;
		private DirectoryInfo updateExtractDirectory;
		private ProgressBar progressBar;
		private int updateParts = 0;
		private String localToRemote;
		private string host;
		private String modHost;
		private int updatePartsDownloaded = 0;
		private bool checkedForBadFiles = false;
		private Queue badFiles;
		private int badFileCorrectionAttempt = 0;
		private TextBlock clientVersionBlock;
		private int extractionFinished = 0;
		public bool isUpdateInProgress = false;
		private bool fullClientUpdate = false;
		private int midVersion;
		private int getUpdateListAttempts = 0;
		private TextBlock statusBlock;
		private TextBlock statusPercentBlock;
		private Dictionary<String, String> updatePartHashes = new Dictionary<string, string>();
		private StreamWriter testlog;

		public ClientUpdater(Game client)
		{
			this.client = client;
			this.updateDirectory = new DirectoryInfo(System.Environment.CurrentDirectory + "\\rdlauncherMabiUpdaterTemp");
			this.updateExtractDirectory = new DirectoryInfo(updateDirectory.FullName + "\\extracted");
		}

		public void checkClientUpdate(ProgressBar progressBar, TextBlock clientVersionBlock, TextBlock statusBlock, TextBlock statusPercentBlock, bool bypassQuestion = false)
		{
			if (client.clientVersion < client.remoteClientVersion && !client.offlineMode)
			{
				MessageBoxResult questionResult = MessageBoxResult.Yes;
				if(!bypassQuestion)
					questionResult = MessageBox.Show("It appears your client is out of date!\nWould you like to update to the latest client version?", "Update", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (questionResult == MessageBoxResult.Yes)
				{
					//Start update
					isUpdateInProgress = true;
					if (client.patchServer.Equals("mabipatch.nexon.net/game"))
					{
						host = "ftp://mabipatch.nexon.net";
					}
					else
					{
						host = "ftp://" + client.patchServer;
					}
					this.clientVersionBlock = clientVersionBlock;
					this.progressBar = progressBar;
					this.statusBlock = statusBlock;
					this.statusPercentBlock = statusPercentBlock;
					statusBlock.Text = "Starting client update";
					statusPercentBlock.Text = "(0%)";
					prepareUpdater();
					if (client.clientVersion < client.remoteClientVersion)
						startClientUpdate(null, null);
				}
				else
				{
					MessageBox.Show("The server shows that a newer client is needed, you will probably not be able to play until you update.");
				}
			}
		}

		private void startClientUpdate(object sender, AsyncCompletedEventArgs e)
		{
			if (updateParts == 0)
			{
				FileInfo patchFile = new FileInfo(updateDirectory.FullName + "\\update.txt");
				if (patchFile.Exists && patchFile.Length > 0)
				{
					readUpdateListFile();
				}
				else
				{
					midVersion = client.remoteClientVersion - (5 * getUpdateListAttempts);
					localToRemote = client.clientVersion + "_to_" + midVersion;
					if (getUpdateListAttempts<6)
					{
						getUpdateListAttempts++;
						statusBlock.Text = "Attempting to get package list for upgrade " + client.clientVersion.ToString() + " to version " + midVersion.ToString();
						downloadFileFromFtp(midVersion + "/" + localToRemote + ".txt", updateDirectory.FullName + "\\update.txt", host, new AsyncCompletedEventHandler(startClientUpdate));
					}
					else
					{
						//Trigger full client redownload
						midVersion = client.remoteClientVersion;
						localToRemote = client.remoteClientVersion + "_full";
						statusBlock.Text = "Starting full client download...";
						downloadFileFromFtp(client.remoteClientVersion + "/" + client.remoteClientVersion + "_full.txt", updateDirectory.FullName + "\\update.txt", host, new AsyncCompletedEventHandler(startClientUpdate));
					}
				}
			}
			else
			{
				downloadClientUpdateParts(null, null);
			}
		}
		private void downloadClientUpdateParts(object sender, AsyncCompletedEventArgs e)
		{
			if (updatePartsDownloaded < updateParts)
			{
				statusBlock.Text = "Downloading update part " + localToRemote + "." + updatePartsDownloaded.ToString("000");
				downloadFileFromFtp(midVersion + "/" + localToRemote + "." + updatePartsDownloaded.ToString("000"), updateDirectory.FullName + "\\" + localToRemote + "." + updatePartsDownloaded.ToString("000"), host, new AsyncCompletedEventHandler(downloadClientUpdateParts));
				updatePartsDownloaded++;
			}
			else
			{
				//Compare hashes with hashes from patch info file
				if (!checkedForBadFiles)
				{
					badFiles = compareHashesForBadFiles();
					checkedForBadFiles = true;
				}
				if (badFiles.Count > 0)
				{
					if (badFileCorrectionAttempt < 4)
					{
						//Redownload files
						badFileCorrectionAttempt++;
						String badFile = (String)badFiles.Dequeue();
						File.Delete(updateDirectory.FullName + "\\" + badFile);
						statusBlock.Text = "Redownloading update part " + badFile;
						downloadFileFromFtp(midVersion + "/" + badFile, updateDirectory.FullName + "\\" + badFile, host, new AsyncCompletedEventHandler(downloadClientUpdateParts));
					}
					else
					{
						statusBlock.Text = "Update Failed!";
						MessageBox.Show("It seems that the package files are corrupted, even after 3 download attempts, please try to patch using a different launcher or try again later...");
					}
				}
				else if (badFileCorrectionAttempt > 0)
				{
					checkedForBadFiles = false;
					downloadClientUpdateParts(null, null);
				}
				if (checkedForBadFiles && badFiles.Count == 0 && File.Exists(updateDirectory.FullName + "\\language.zip"))
				{
					//Download complete merge and install
					statusBlock.Text = "Merging update parts";
					mergeUpdateParts();
				}
				else
				{
					statusBlock.Text = "Downloading language pack for the new version";
					downloadFileFromFtp(midVersion + "/" + midVersion + "_language.p_", updateDirectory.FullName + "\\language.zip", host, new AsyncCompletedEventHandler(downloadClientUpdateParts));
				}
			}
		}

		private void finishUpdate(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Cancelled)
			{
				MessageBox.Show("The extraction operation has been canceled!");
			}
			else if (!(e.Error == null))
			{
				MessageBox.Show("The background worker operation extracting the patch has encountered an error, please retry the patch!");
			}
			else
			{
				if (extractionFinished > 0)
				{
					statusBlock.Text = "Installing updates";
					testlog.Close();
					moveExtractedDataToClient();
				}
				else
				{
					extractionFinished++;
				}
			}
		}

		private void updateCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Cancelled)
			{
				MessageBox.Show("The patch install operation was canceled!");
				statusBlock.Text = "Update Interrupted";
				statusPercentBlock.Text = "";
				isUpdateInProgress = false;
			}
			else if (!(e.Error == null))
			{
				MessageBox.Show("The background worker operation moving the patch data has encountered an error, please retry the patch!");
				statusBlock.Text = "Update Failed!";
				statusPercentBlock.Text = "";
				isUpdateInProgress = false;
			}
			else
			{
				updateDirectory.Delete(true);
				isUpdateInProgress = false;
				client.writeVersionData(midVersion, clientVersionBlock);
				if (midVersion == client.remoteClientVersion)
				{
					MessageBox.Show("The patch has been completed successfully, you may now launch the client!");
					statusBlock.Text = "Ready to launch!";
					statusPercentBlock.Text = "";
				}
				else
				{
					//Autostart next version update after the catchup update
					statusBlock.Text = "Starting version " + client.remoteClientVersion.ToString() + " update";
					checkClientUpdate(progressBar, clientVersionBlock, statusBlock, statusPercentBlock, true);
				}
			}
		}

		private Queue compareHashesForBadFiles()
		{
			Queue result = new Queue();
			for (int i = 0; i < updateParts; i++)
			{
				FileInfo file = new FileInfo(updateDirectory.FullName + "\\" + localToRemote + "." + i.ToString("000"));
				String fileHash = BitConverter.ToString(System.Security.Cryptography.MD5.Create().ComputeHash(file.OpenRead())).ToLower().Replace("-", "");
				if (!fileHash.Equals(updatePartHashes[file.Name]))
				{
					result.Enqueue(file.Name);
				}
			}
			return result;
		}

		private void mergeUpdateParts()
		{
			BackgroundWorker merger = new BackgroundWorker();
			merger.WorkerReportsProgress = true;
			merger.RunWorkerCompleted += new RunWorkerCompletedEventHandler(extractUpdateZips);
			merger.ProgressChanged += new ProgressChangedEventHandler(progressUpdate);
			merger.DoWork += (o, e) =>
			{
				FileInfo output = new FileInfo(updateDirectory.FullName + "\\update.zip");
				FileStream outputStream = output.OpenWrite();
				for (int i = 0; i < updateParts; i++)
				{
					merger.ReportProgress((i / updateParts) * 100);
					CopyStream(outputStream, File.OpenRead(updateDirectory.FullName + "\\" + localToRemote + "." + i.ToString("000")));
				}
				outputStream.Close();
			};
			merger.RunWorkerAsync();
		}

		private void extractUpdateZips(Object sender, RunWorkerCompletedEventArgs e)
		{
			testlog = new StreamWriter(File.OpenWrite("testextlog.txt"));
			statusBlock.Text = "Extracting update files to temp directory";
			ExtractFile(updateDirectory.FullName + "\\update.zip", updateExtractDirectory.FullName);
			ExtractFile(updateDirectory.FullName + "\\language.zip", updateExtractDirectory.FullName + "\\package\\");
		}

		void CopyStream(Stream destination, Stream source)
		{
			int count;
			byte[] buffer = new byte[1024];
			while ((count = source.Read(buffer, 0, buffer.Length)) > 0)
				destination.Write(buffer, 0, count);
		}

		private void moveExtractedDataToClient()
		{
			BackgroundWorker worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(updateCompleted);
			worker.ProgressChanged += new ProgressChangedEventHandler(progressUpdate);
			worker.DoWork += (o, e) =>
			{
				int fileTransferred = 0;
				foreach (DirectoryInfo dir in updateExtractDirectory.GetDirectories("*", SearchOption.AllDirectories))
					Directory.CreateDirectory(dir.FullName.Replace(updateExtractDirectory.FullName, client.settings.clientInstallDirectory + "\\"));
				FileInfo[] files = updateExtractDirectory.GetFiles("*", SearchOption.AllDirectories);
				foreach (FileInfo file in files)
				{
					file.CopyTo(Path.Combine(client.settings.clientInstallDirectory, file.FullName.Replace(updateExtractDirectory.FullName + "\\", "")), true);
					fileTransferred++;
					worker.ReportProgress((fileTransferred / files.Length) * 100);
					file.Delete();
				}
			};

			worker.RunWorkerAsync();
		}

		public void ExtractFile(string zipToUnpack, string unpackDirectory)
		{
			BackgroundWorker worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(finishUpdate);
			worker.ProgressChanged += new ProgressChangedEventHandler(progressUpdate);
			worker.DoWork += (o, e) =>
			{
				using (ZipFile zip = ZipFile.Read(zipToUnpack))
				{
					zip.ExtractProgress += (s, ev) =>
					{
						if(ev.TotalBytesToTransfer>0)
							worker.ReportProgress((Int32)((ev.BytesTransferred/ev.TotalBytesToTransfer)*100));
					};
					zip.ExtractAll(unpackDirectory, ExtractExistingFileAction.OverwriteSilently);
				}
			};

			worker.RunWorkerAsync();
		}

		public void downloadFileFromFtp(String pathToFile, String pathToSave, String host, AsyncCompletedEventHandler returnEvent)
		{
			WebClient w = new WebClient();
			w.DownloadFileCompleted += returnEvent;
			w.DownloadProgressChanged += new DownloadProgressChangedEventHandler(progressUpdate);
			w.DownloadFileAsync(new Uri(host + "/" + pathToFile), pathToSave);
		}

		private void progressUpdate(object sender, ProgressChangedEventArgs e)
		{
			progressBar.Value = e.ProgressPercentage;
			statusPercentBlock.Text = "(" + e.ProgressPercentage.ToString() + "%)";
		}

		private void readUpdateListFile()
		{
			String[] updateList = File.ReadAllLines(updateDirectory.FullName + "\\update.txt");
			updateParts = int.Parse(updateList[0]);
			for (int i = 1; i < updateList.Length; i++)
			{
				String[] lineTemp = updateList[i].Split(',');
				updatePartHashes.Add(lineTemp[0].Trim(), lineTemp[2].Trim());
			}
			startClientUpdate(null, null);
		}

		private void prepareUpdater()
		{
			if (updateDirectory.Exists == true)
			{
				updateDirectory.Delete(true);
				updateDirectory.Create();
				updateExtractDirectory.Create();
			}
			else
			{
				updateDirectory.Create();
				updateExtractDirectory.Create();
			}
			//Reset clientUpdater variables for multiple updates
			updateParts = 0;
			updatePartsDownloaded = 0;
			checkedForBadFiles = false;
			extractionFinished = 0;
			updatePartHashes.Clear();
			getUpdateListAttempts = 0;
			badFileCorrectionAttempt = 0;
		}
	}
}
