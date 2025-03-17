# Commentus

Commentus is an application that allows you to create rooms for teams and assign tasks easily.

## Author

This project was created by [Tomas Trachta](https://www.github.com/tomas-trachta).

## Demo from admin's perspective

![Commentus admin showcase](https://github.com/tomas-trachta/Commentus/blob/main/Commentus/Resources/commentus_admin_showcase.gif)

## Getting started

To get started with Commentus, you can clone the project with the following command:

```
git clone https://github.com/ponny159/commentus
```


After cloning the project, you need to perform the following steps:

1. Publish the project locally with a key and install that key.

2. Install the package and set up a MySql database.

3. Start application. Click on the options and select "Set up database". Connect to your MySql database, and Commentus will set up tables for you.

4. Register for an account, and then set the "IsAdmin" column in the "users" table of your row to 1. Once you do that, you can deliver admin privileges from this account by going to Home Page -> Options -> Give admin privileges.

After you set up the database connection, the connection string will be stored encrypted in the apps folder and automatically loaded. If you want to store it securely, open the source code and go to ```Cryptography -> ConfigManager```, and set up your own ```Key``` and ```IV```.

Notes:

- You can enter images into chat, but because there is no viable multiplatform way to paste images from clipboard, paste the absolute path/URL instead. Supported formats are png and jpeg.

- You can style the content of tasks with HTML and CSS.


## Technologies

**RDBMS:** MySql

**Application:** .NET 6.0, .NET MAUI

**Dependencies:** Community toolkit MVVM, Microsoft SqlClient, MySql.Data, SkiaSharp

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://github.com/Ponny159/Commentus/blob/main/LICENSE.txt)
