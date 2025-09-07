# Vehicle Auction API

This RESTful API powers a vehicle auction platform. It allows users to create auctions by listing cars with details like make, model, year, and a minimum price. Buyers can browse and place bids. The system also manages the auction lifecycle, including a robust process for handling non-paying bidders with a second-chance offer system and user penalties to ensure fairness.

## Features

- **Auction Creation**: Users can list vehicles for auction with a specified brand, model, year, minimum price, and expiration date.
- **Bidding System**: Buyers can place bids on active auctions. The API validates bids to ensure they are higher than the current highest bid and meet the minimum price.
- **Real-Time Updates**: The system is designed to provide real-time updates on bids and auction status.
- **Non-Payment Handling**: A robust system is in place to manage non-paying bidders, including "second-chance" offers to the next highest bidder and a strike system to penalize non-compliant users.
- **User Authentication**: Secure user registration and login are handled to ensure that only authenticated users can perform actions like bidding and listing vehicles.

## About the Project

This document provides a high-level overview of the API's functionality. For detailed specifications, including request/response examples, please refer to the API documentation.
