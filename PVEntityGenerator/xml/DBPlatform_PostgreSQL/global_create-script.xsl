<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  version="1.0"
>

<xsl:import href="db-platform_defaults.xsl"/>

<xsl:output method="text" encoding="ISO-8859-1"/>


<xsl:template match="/pventitygenerator-data">

  <!-- Header -->
  <xsl:text>/*************************************************************************************

  Create script
  DB Platform:  PostgreSQL

**************************************************************************************/


</xsl:text>

  <!-- Drop table statements -->
  <!--
  as PostgreSQL does not support DROP ... IF EXISTS we have to create
  a stored procedure drop_if_exists(<tablename>) to do the job
  -->
  <xsl:text>/* Drop existing tables and defaults (if needed)  /*

/*
*****************************************************
                  !!! ATTENTION !!!
as PostgreSQL does not support DROP ... IF EXISTS
this script will create a stored procedure
drop_if_exists('tablename','DROP tablename CASCADE')
to do the job
please make sure that you have enabled
the pgsql support in your database
*****************************************************
*/

  CREATE OR REPLACE FUNCTION drop_if_exists(varchar,varchar)
    RETURNS boolean
AS '
DECLARE
  strTableName ALIAS FOR $1;
  strDropCMD   ALIAS FOR $2;
  intCount int4;
BEGIN
    SELECT COUNT(*) FROM pg_tables WHERE tablename like LOWER(strTableName) INTO intCount;
    IF intCount>0 THEN
      EXECUTE strDropCMD;
      RETURN true;
    ELSE
      RETURN false;
    END IF;
END;'
LANGUAGE 'plpgsql' ;
COMMENT ON FUNCTION drop_if_exists(varchar,varchar)
IS 'EXECUTE SQL Command arg2 IF TABLE arg1 EXISTS';
/* ******************************************************* */

</xsl:text>
  <xsl:for-each select="$project-settings/entity-export/export-entities/export-entity[@export-structure='true' or @export-drop='true']">
    <xsl:sort select="@sort-no" data-type="number" order="descending"/>
    <xsl:sort select="@entity" order="descending"/>
    <xsl:apply-templates select="." mode="drop"/>
  </xsl:for-each>

  <xsl:text>
/*** Drop the stored procedure. ***/
DROP FUNCTION drop_if_exists(varchar,varchar);
  </xsl:text>

  <!-- Create table statements -->
  <xsl:for-each select="$project-settings/entity-export/export-entities/export-entity[@export-structure='true']">
    <xsl:sort select="@sort-no" data-type="number"/>
    <xsl:sort select="@entity"/>
    <xsl:apply-templates select="$db-definition/entities/entity[@name=current()/@entity]" mode="create"/>
  </xsl:for-each>

</xsl:template>

</xsl:stylesheet>
