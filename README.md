# Console User Management System

A C# console application implementing a secure authentication and admin system with role-based access control, session management, and audit logging.

This project focuses on correct architecture, security decisions, and clean separation of responsibilities rather than feature quantity.


## Key Features
### Users

Register, login, logout

Change own username and password

Delete own account

Forced password change when required



### Admins

View audit logs

View all users and user details

Delete users

Change user roles

Reset user passwords with temporary credentials

Force password change on next login



## Security Highlights

Passwords hashed using PBKDF2

Centralized role-based authorization guards

Single-session model via Session.CurrentUser

Forced password reset flow with temporary passwords

Full audit trail of security-relevant actions



## Architecture

Session-based identity for UI actions

Parameter-based identity for reusable domain logic

Clear separation between:

UI / Menus

Authorization guards

Domain logic

Storage

Utilities

Nullable reference types enabled and handled safely



## Persistence

Users and logs stored as JSON files

Files are created automatically if missing

Logs act as an authoritative audit trail



## Design Goals

Correct authorization over convenience

Centralized security rules

Defensive programming

Feature completeness over feature creep



## Status

Feature-complete and frozen.

Built as a learning and portfolio project to demonstrate:

Authentication & authorization design

Secure password handling

Session modeling

Clean architecture decisions
