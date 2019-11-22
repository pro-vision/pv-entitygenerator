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
  DB Platform:  MySQL

**************************************************************************************/


</xsl:text>

  <!-- Drop table statements -->
  <xsl:text>/* Drop existing tables and defaults (if needed) */

SET FOREIGN_KEY_CHECKS=0;

</xsl:text>
  <xsl:for-each select="$project-settings/entity-export/export-entities/export-entity[@export-structure='true' or @export-drop='true']">
    <xsl:sort select="@sort-no" data-type="number" order="descending"/>
    <xsl:sort select="@entity" order="descending"/>
    <xsl:apply-templates select="." mode="drop"/>
  </xsl:for-each>

  <xsl:text>
SET FOREIGN_KEY_CHECKS=1;
</xsl:text>

  <!-- Create table statements -->
  <xsl:for-each select="$project-settings/entity-export/export-entities/export-entity[@export-structure='true']">
    <xsl:sort select="@sort-no" data-type="number"/>
    <xsl:sort select="@entity"/>
    <xsl:apply-templates select="$db-definition/entities/entity[@name=current()/@entity]" mode="create"/>
  </xsl:for-each>

</xsl:template>

</xsl:stylesheet>
