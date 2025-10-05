CREATE OR REPLACE FUNCTION velocity.set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at := CURRENT_TIMESTAMP;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER set_vehicle_updated_at
BEFORE UPDATE ON velocity._vehicle
FOR EACH ROW
EXECUTE FUNCTION velocity.set_updated_at();
