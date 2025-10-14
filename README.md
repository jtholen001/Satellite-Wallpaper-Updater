# Satellite-Wallpaper-Updater

[![.NET](https://github.com/jtholen001/Satellite-Wallpaper-Updater/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/jtholen001/Satellite-Wallpaper-Updater/actions/workflows/dotnet.yml)

A lightweight Windows Forms application that runs in the system tray and automatically fetches the latest **full disk images of Earth** from **NESDIS GOES satellites**.

The app runs quietly in the background, updating images on a timer and keeping only the most recent photos for efficient storage. Deployment and updates are handled via **ClickOnce**, hosted on my personal website

---

## ✨ Features

* 🛰️ Automatically downloads the latest **NESDIS GOES full disk images**.
* ⏱️ Configurable timer for periodic updates.
* 📂 Maintains only the **most recent images** to save disk space.
* 🖥️ Runs seamlessly in the **Windows system tray**.
* 🔄 Easy installation and auto-updates with **ClickOnce**.

---

## 🚀 Installation

1. Navigate to the ClickOnce installer hosted at:
   **[applications.tholen.cc](https://applications.tholen.cc/SatelliteWallpaper/SatelliteWallpaperUpdater.application)** 
2. Open the downloaded file to install
3. The app will appear in your **system tray** after launch.

---

## ⚙️ Usage

* Double-click the tray icon to see the latest photo
* Right-click on the tray icon to close the app.
 
The app will automatically download new full disk images when available and overwrite the older ones.

---

## 🛠️ Technology Stack

* **C#** (Windows Forms)
* **ClickOnce** for deployment & updates
* **Windows System Tray Integration**


---

## 📸 Data Source

Images are sourced from **[NESDIS (NOAA Satellite and Information Service)](https://www.nesdis.noaa.gov/)** via their public file share.

---

## 📦 Roadmap

* [X] User-configurable download location
* [ ] Multi-resolution image support
* [ ] Desktop wallpaper auto-update option
	* Currently only triggered manually via the start menu, or on a computer restart (via startup apps).
* [ ] Logging & error handling improvements

---

## 🤝 Contributing

Currently, this is a personal project, but suggestions and feature requests are welcome!

---

## 📄 License

This project is licensed under the **MIT License** – feel free to use and modify.


