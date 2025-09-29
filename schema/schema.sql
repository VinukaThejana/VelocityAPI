CREATE SCHEMA IF NOT EXISTS velocity;

CREATE EXTENSION IF NOT EXISTS pgx_ulid;

CREATE TABLE IF NOT EXISTS velocity._user (
  id CHAR(26) PRIMARY KEY DEFAULT public.gen_ulid(),
  email VARCHAR(255) NOT NULL UNIQUE,
  name VARCHAR(255) NOT NULL,
  photo_url VARCHAR(255) NOT NULL,
  nic VARCHAR(255) NOT NULL UNIQUE,
  strikes INT NOT NULL DEFAULT 0 CONSTRAINT max_strikes CHECK (strikes >= 0 AND strikes <= 3),
  password VARCHAR(255) NOT NULL,
  email_verified BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_user_email ON velocity._user(email);
CREATE INDEX idx_user_nic ON velocity._user(nic);

CREATE TABLE IF NOT EXISTS velocity._bank_details (
  id CHAR(26) PRIMARY KEY DEFAULT public.gen_ulid(),
  user_id CHAR(26) NOT NULL UNIQUE REFERENCES velocity._user(id) ON DELETE CASCADE ON UPDATE CASCADE,
  account_holder_name VARCHAR(255) NOT NULL,
  account_number VARCHAR(20) NOT NULL,
  bank_name VARCHAR(100) NOT NULL,
  branch_name VARCHAR(100),
  ifsc_code VARCHAR(50)
);

CREATE INDEX idx_bank_details_user_id ON velocity._bank_details(user_id);

CREATE TABLE IF NOT EXISTS velocity._brand (
  id SMALLINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
  slug VARCHAR(50) NOT NULL UNIQUE
    CHECK(
      char_length(slug) <= 50
      AND slug ~ '^[A-Za-z0-9-]+$'
    ),
  name VARCHAR(255) NOT NULL
);

CREATE INDEX idx_brand_slug ON velocity._brand(slug);

CREATE TABLE IF NOT EXISTS velocity._vehicle_type (
  id SMALLINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
  slug VARCHAR(50) NOT NULL UNIQUE
    CHECK(
      char_length(slug) <= 50
      AND slug ~ '^[A-Za-z0-9-]+$'
    ),
  name VARCHAR(255) NOT NULL
);

CREATE INDEX idx_vehicle_type_slug ON velocity._vehicle_type(slug);

CREATE TABLE IF NOT EXISTS velocity._vehicle (
  id CHAR(26) PRIMARY KEY DEFAULT public.gen_ulid(),

  owner_id CHAR(26) NOT NULL REFERENCES velocity._user(id) ON DELETE CASCADE ON UPDATE CASCADE,
  brand_id SMALLINT NOT NULL REFERENCES velocity._brand(id) ON DELETE RESTRICT ON UPDATE CASCADE,
  vehicle_type_id SMALLINT NOT NULL REFERENCES velocity._vehicle_type(id) ON DELETE RESTRICT ON UPDATE CASCADE,

  name VARCHAR(255) NOT NULL,
  photos TEXT[] NOT NULL DEFAULT '{}',
  model VARCHAR(255) NOT NULL,
  color VARCHAR(100) NOT NULL,
  license_plate VARCHAR(15) NOT NULL UNIQUE,
  year INT NOT NULL CHECK (year >= 1886 AND year <= EXTRACT(YEAR FROM CURRENT_DATE) + 1),
  mileage INT NOT NULL CHECK (mileage >= 0),
  description TEXT NOT NULL,

  is_active BOOLEAN NOT NULL DEFAULT TRUE,

  created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_vehicle_owner_id ON velocity._vehicle(owner_id);
CREATE INDEX idx_vehicle_brand_id ON velocity._vehicle(brand_id);
CREATE INDEX idx_vehicle_vehicle_type_id ON velocity._vehicle(vehicle_type_id);
CREATE INDEX idx_vehicle_license_plate ON velocity._vehicle(license_plate);
CREATE INDEX idx_vehicle_is_active ON velocity._vehicle(is_active);

CREATE TABLE IF NOT EXISTS velocity._auctions (
  id CHAR(26) PRIMARY KEY DEFAULT public.gen_ulid(),
  vehicle_id CHAR(26) NOT NULL REFERENCES velocity._vehicle(id) ON DELETE CASCADE ON UPDATE CASCADE,
  seller_id CHAR(26) NOT NULL REFERENCES velocity._user(id) ON DELETE CASCADE ON UPDATE CASCADE,

  starting_price DECIMAL(12, 2) NOT NULL CHECK (starting_price >= 0),
  status VARCHAR(50) NOT NULL DEFAULT 'active' CHECK (status IN ('active', 'expired', 'sold', 'cancelled')),

  start_time TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
  expiration BIGINT NOT NULL CHECK (expiration > EXTRACT(EPOCH FROM NOW()))
);

CREATE UNIQUE INDEX unique_active_auction_per_vehicle ON velocity._auctions(vehicle_id) WHERE status = 'active';

CREATE INDEX idx_auctions_vehicle_id ON velocity._auctions(vehicle_id);
CREATE INDEX idx_auctions_seller_id ON velocity._auctions(seller_id);
CREATE INDEX idx_auctions_status ON velocity._auctions(status);
CREATE INDEX idx_auctions_start_time ON velocity._auctions(start_time);
CREATE INDEX idx_auctions_expiration ON velocity._auctions(expiration);

CREATE TABLE IF NOT EXISTS velocity._bids (
  id CHAR(26) PRIMARY KEY DEFAULT public.gen_ulid(),
  auction_id CHAR(26) NOT NULL REFERENCES velocity._auctions(id) ON DELETE CASCADE ON UPDATE CASCADE,
  bidder_id CHAR(26) NOT NULL REFERENCES velocity._user(id) ON DELETE CASCADE ON UPDATE CASCADE,

  amount DECIMAL(12, 2) NOT NULL CHECK (amount > 0),

  bid_time TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_bids_auction_id ON velocity._bids(auction_id);
CREATE INDEX idx_bids_bidder_id ON velocity._bids(bidder_id);
CREATE INDEX idx_bids_bid_time ON velocity._bids(bid_time);
CREATE INDEX idx_bids_amount ON velocity._bids(auction_id, amount DESC);

CREATE TABLE IF NOT EXISTS velocity._ownership_transfers (
  id CHAR(26) PRIMARY KEY DEFAULT public.gen_ulid(),
  auction_id CHAR(26) REFERENCES velocity._auctions(id) ON DELETE CASCADE ON UPDATE CASCADE,
  vehicle_id CHAR(26) NOT NULL REFERENCES velocity._vehicle(id) ON DELETE CASCADE ON UPDATE CASCADE,
  from_user_id CHAR(26) NOT NULL REFERENCES velocity._user(id) ON DELETE CASCADE ON UPDATE CASCADE,
  to_user_id CHAR(26) NOT NULL REFERENCES velocity._user(id) ON DELETE CASCADE ON UPDATE CASCADE,

  transfer_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_ownership_transfers_vehicle_id ON velocity._ownership_transfers(vehicle_id);
CREATE INDEX idx_ownership_transfers_from_user_id ON velocity._ownership_transfers(from_user_id);
CREATE INDEX idx_ownership_transfers_to_user_id ON velocity._ownership_transfers(to_user_id);
CREATE INDEX idx_ownership_transfers_auction_id ON velocity._ownership_transfers(auction_id);
CREATE INDEX idx_ownership_transfers_transfer_date ON velocity._ownership_transfers(transfer_date);

CREATE TABLE IF NOT EXISTS velocity._transactions (
  id CHAR(26) PRIMARY KEY DEFAULT public.gen_ulid(),
  auction_id CHAR(26) NOT NULL UNIQUE REFERENCES velocity._auctions(id) ON DELETE CASCADE ON UPDATE CASCADE,
  buyer_id CHAR(26) NOT NULL REFERENCES velocity._user(id) ON DELETE CASCADE ON UPDATE CASCADE,
  seller_id CHAR(26) NOT NULL REFERENCES velocity._user(id) ON DELETE CASCADE ON UPDATE CASCADE,

  amount DECIMAL(12, 2) NOT NULL CHECK (amount > 0),
  payment_proof VARCHAR(255),
  payment_status VARCHAR(50) NOT NULL DEFAULT 'pending' CHECK (payment_status IN ('pending', 'completed', 'failed', 'refunded')),

  transaction_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_transactions_auction_id ON velocity._transactions(auction_id);
CREATE INDEX idx_transactions_buyer_id ON velocity._transactions(buyer_id);
CREATE INDEX idx_transactions_seller_id ON velocity._transactions(seller_id);
CREATE INDEX idx_transactions_payment_status ON velocity._transactions(payment_status);
