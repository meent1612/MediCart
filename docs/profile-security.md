# User Profile Security Architecture

## 1. Email Address Immutability
User email addresses cannot be modified from the self-service profile page to prevent account takeover vectors.

## 2. Server-Side Enforcement
The `UserProfileController.UpdateProfile` action explicitly excludes the `Email` property from being bound or updated.

