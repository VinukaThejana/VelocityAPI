
CREATE TABLE IF NOT EXISTS _user (
  id CHAR(26) PRIMARY KEY DEFAULT gen_ulid(),
  email VARCHAR(255) NOT NULL UNIQUE,
  name VARCHAR(255) NOT NULL,
  photo_url VARCHAR(255) NOT NULL,
  nic VARCHAR(255) NOT NULL UNIQUE,
  strikes INT NOT NULL DEFAULT 0 CONSTRAINT max_strikes CHECK (strikes >= 0 AND strikes <= 3),
  password VARCHAR(255) NOT NULL,
  email_verified BOOLEAN NOT NULL DEFAULT FALSE,
);

CREATE INDEX idx_user_email ON _user(email);
CREATE INDEX idx_user_nic ON _user(nic);

