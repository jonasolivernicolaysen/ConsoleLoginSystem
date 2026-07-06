# ConsoleLoginSystem

## ConsoleLoginSystem in an easy-to-use authentication and authorization application

## Features include
- Secure password hashing
- Admin control panel
- Role-based authorization
- Register and login retry logic
- Functional decomposition

### Register and login
<p align="left">
  <img src="assets/register.png" width="380">
  <img src="assets/login.png" width="380">
</p>

### Admin control panel
The first user created will automatically become an admin
<p align="left">
  <img src="assets/admin_control_panel.png" width="300">
</p>

The admin can perform neccessary admin operations such as viewing the logs
<p align="left">
  <img src="assets/admin_viewlogs.png">
</p>


## Running with Docker

Build the image and start the application:

```bash
docker compose run --build --rm consoleapp
```

On subsequent runs, if no source code has changed, you don't need `--build`:

```bash
docker compose run --rm consoleapp
```
```

Open your browser and navigate to http://localhost:5000
