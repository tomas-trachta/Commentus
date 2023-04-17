
# Commentus

Create rooms for teams and assign tasks easily.


## Author

[@Ponny159 (T. Trachta)](https://www.github.com/ponny159)


## Demo from admins perspective

![](https://github.com/Ponny159/Commentus/blob/main/Commentus/Resources/commentus_admin_showcase.gif)


## Run Locally

Clone the project

```bash
  git clone https://github.com/ponny159/commentus
```

Publish it locally with key and install that key.  
Then install the package.  

Set up MySql database.  

Run the application, click on options and select 'Set up database'.  
Connect to your MySql database.  
Commentus will set up tables for you.  
Register. Now you need to set 'IsAdmin' column in 'users' table of your row to 1.  
Once you do that, you can deliver admin privileges from this account using:  
``` Home Page -> Options -> Give admin privileges ```  

Once you set up database connection, the connection string will be stored encrypted in apps folder and automatically loaded.  
If you want to store it securely, open source code and go to  
``` Cryptography -> ConfigManager ``` and set up your own ``` Key ``` and ``` IV ```
## Technologies

**RDBMS:** MySql

**Application:** .NET 6.0, .NET MAUI  

**Dependencies:** Community toolkit MVVM, Microsoft SqlClient, MySql.Data, SkiaSharp

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://raw.githubusercontent.com/Ponny159/commentus/main/license.txt)

