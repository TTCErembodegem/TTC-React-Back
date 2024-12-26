UPDATE speler SET LastName='Al Hussein' where ID=75;
DELETE FROM speler WHERE ID=724;
DELETE FROM speler WHERE ID=714;
DELETE FROM speler WHERE ID=594;
-- UPDATE parameter SET VALUE='2024' WHERE sleutel='year'; -- Do this with admin/configParams year update
UPDATE parameter SET VALUE='info@ttc-aalst.be' WHERE sleutel='FromEmail';
DELETE FROM parameter WHERE sleutel='frenoy_wsdlUrlVTTL';

-- TODO: check googleMapsUrl -- pointing to correct place?
-- TODO: check frenoyLogin/pwd -- still correct?