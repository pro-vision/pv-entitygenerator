<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  version="1.0"
>

<xsl:import href="db-platform_defaults.xsl"/>

<xsl:output method="text" encoding="ISO-8859-1"/>


<xsl:template match="/pventitygenerator-data">

<xsl:variable name="generate-db-storedproc" select="$project-settings/platforms/platform[@name=../@selected]/parameters/parameter[@name='generate-db-storedproc']"/>

<xsl:if test="not($generate-db-storedproc) or $generate-db-storedproc='true'">

<!-- Header -->
<xsl:text>/*************************************************************************************

  Stored Proc Script
  DB Platform:  SQL Server

**************************************************************************************/

</xsl:text>

<xsl:apply-templates select="$db-definition/entities/entity"/>

</xsl:if>

</xsl:template>


<!-- ***** Stored Proc für jedes Entity generieren ***** -->
<xsl:template match="entity">

  <xsl:variable name="generate-entity" select="key('generate-entity', @name)"/>

  <xsl:if test="$generate-entity/parameters/parameter[@name='generate-entity']='true'">

    <xsl:apply-templates select="." mode="storedproc-findbyprimarykey"/>
    <xsl:apply-templates select="." mode="storedproc-create"/>
    <xsl:apply-templates select="." mode="storedproc-store"/>
    <xsl:apply-templates select="." mode="storedproc-remove"/>

  </xsl:if>

</xsl:template>


<!-- ***** Stored Proc für default findByPrimaryKey method ***** -->
<xsl:template match="entity" mode="storedproc-findbyprimarykey">

  <xsl:text>
/*** </xsl:text><xsl:value-of select="@name"/><xsl:text>.fndPk ***/
IF EXISTS (SELECT name FROM sysobjects WHERE name=N'sp_</xsl:text><xsl:value-of select="@name"/><xsl:text>_fndPk' AND type='P')
  DROP PROCEDURE sp_</xsl:text><xsl:value-of select="@name"/><xsl:text>_fndPk
GO
CREATE PROCEDURE sp_</xsl:text><xsl:value-of select="@name"/><xsl:text>_fndPk
  @</xsl:text><xsl:value-of select="attributes/attribute[position()=1]/@name"/><xsl:text> </xsl:text>
  <xsl:apply-templates select="attributes/attribute[position()=1]" mode="sql-type"/><xsl:text>
AS
SELECT </xsl:text>
  <xsl:for-each select="attributes/attribute">
    <xsl:value-of select="@name"/>
    <xsl:if test="position() &lt; last()">
      <xsl:text>, </xsl:text>
    </xsl:if>
  </xsl:for-each>
  <xsl:text>
FROM </xsl:text><xsl:value-of select="@name"/><xsl:text>
WHERE (</xsl:text><xsl:value-of select="attributes/attribute[position()=1]/@name"/><xsl:text>=@</xsl:text>
  <xsl:value-of select="attributes/attribute[position()=1]/@name"/><xsl:text>)
GO
</xsl:text>
  <xsl:if test="$db-platform/parameters/parameter[@name='grant-to']!=''">
    <xsl:text>GRANT EXECUTE ON sp_</xsl:text><xsl:value-of select="@name"/><xsl:text>_fndPk TO </xsl:text><xsl:value-of select="$db-platform/parameters/parameter[@name='grant-to']"/><xsl:text>
GO
</xsl:text>
  </xsl:if>

</xsl:template>


<!-- ***** Stored Proc für object creation ***** -->
<xsl:template match="entity" mode="storedproc-create">

  <xsl:text>
/*** </xsl:text><xsl:value-of select="@name"/><xsl:text>.crt ***/
IF EXISTS (SELECT name FROM sysobjects WHERE name=N'sp_</xsl:text><xsl:value-of select="@name"/><xsl:text>_crt' AND type='P')
  DROP PROCEDURE sp_</xsl:text><xsl:value-of select="@name"/><xsl:text>_crt
GO
CREATE PROCEDURE sp_</xsl:text><xsl:value-of select="@name"/><xsl:text>_crt
</xsl:text>
  <xsl:for-each select="attributes/attribute[@type!='VSTAMP']">
    <xsl:text>  @</xsl:text><xsl:value-of select="@name"/><xsl:text> </xsl:text>
    <xsl:apply-templates select="." mode="sql-type"/><xsl:text>,
</xsl:text>
  </xsl:for-each>
  <xsl:text>  @new_vstamp INTEGER
AS
INSERT INTO </xsl:text><xsl:value-of select="@name"/><xsl:text>(</xsl:text>
  <xsl:for-each select="attributes/attribute">
    <xsl:value-of select="@name"/>
    <xsl:if test="position() &lt; last()">
      <xsl:text>, </xsl:text>
    </xsl:if>
  </xsl:for-each>
  <xsl:text>)
VALUES(</xsl:text>
  <xsl:for-each select="attributes/attribute">
    <xsl:choose>
      <xsl:when test="@type='VSTAMP'"><xsl:text>@new_vstamp</xsl:text></xsl:when>
      <xsl:otherwise><xsl:value-of select="concat('@',@name)"/></xsl:otherwise>
    </xsl:choose>
    <xsl:if test="position() &lt; last()">
      <xsl:text>, </xsl:text>
    </xsl:if>
  </xsl:for-each>
  <xsl:text>)
GO
</xsl:text>
  <xsl:if test="$db-platform/parameters/parameter[@name='grant-to']!=''">
    <xsl:text>GRANT EXECUTE ON sp_</xsl:text><xsl:value-of select="@name"/><xsl:text>_crt TO </xsl:text><xsl:value-of select="$db-platform/parameters/parameter[@name='grant-to']"/><xsl:text>
GO
</xsl:text>
  </xsl:if>

</xsl:template>


<!-- ***** Stored Proc für objekt update ***** -->
<xsl:template match="entity" mode="storedproc-store">

  <xsl:text>
/*** </xsl:text><xsl:value-of select="@name"/><xsl:text>.store ***/
IF EXISTS (SELECT name FROM sysobjects WHERE name=N'sp_</xsl:text><xsl:value-of select="@name"/><xsl:text>_store' AND type='P')
  DROP PROCEDURE sp_</xsl:text><xsl:value-of select="@name"/><xsl:text>_store
GO
CREATE PROCEDURE sp_</xsl:text><xsl:value-of select="@name"/><xsl:text>_store
</xsl:text>
  <xsl:for-each select="attributes/attribute">
    <xsl:text>  @</xsl:text><xsl:value-of select="@name"/><xsl:text> </xsl:text>
    <xsl:apply-templates select="." mode="sql-type"/><xsl:text>,
</xsl:text>
  </xsl:for-each>
  <xsl:text>  @new_vstamp INTEGER
AS
UPDATE </xsl:text><xsl:value-of select="@name"/><xsl:text>
SET </xsl:text>
  <xsl:for-each select="attributes/attribute[position()!=1]">
    <xsl:choose>
      <xsl:when test="@type='VSTAMP'"><xsl:text>vstamp=@new_vstamp</xsl:text></xsl:when>
      <xsl:otherwise><xsl:value-of select="concat(@name,'=@',@name)"/></xsl:otherwise>
    </xsl:choose>
    <xsl:if test="position() &lt; last()">
      <xsl:text>, </xsl:text>
    </xsl:if>
  </xsl:for-each>
  <xsl:text>
WHERE (</xsl:text><xsl:value-of select="attributes/attribute[position()=1]/@name"/><xsl:text>=@</xsl:text><xsl:value-of select="attributes/attribute[position()=1]/@name"/><xsl:text>) AND (vstamp=@vstamp)
GO
</xsl:text>
  <xsl:if test="$db-platform/parameters/parameter[@name='grant-to']!=''">
    <xsl:text>GRANT EXECUTE ON sp_</xsl:text><xsl:value-of select="@name"/><xsl:text>_store TO </xsl:text><xsl:value-of select="$db-platform/parameters/parameter[@name='grant-to']"/><xsl:text>
GO
</xsl:text>
  </xsl:if>

</xsl:template>


<!-- ***** Stored Proc für object remove ***** -->
<xsl:template match="entity" mode="storedproc-remove">

  <xsl:text>
/*** </xsl:text><xsl:value-of select="@name"/><xsl:text>.rmv ***/
IF EXISTS (SELECT name FROM sysobjects WHERE name=N'sp_</xsl:text><xsl:value-of select="@name"/><xsl:text>_rmv' AND type='P')
  DROP PROCEDURE sp_</xsl:text><xsl:value-of select="@name"/><xsl:text>_rmv
GO
CREATE PROCEDURE sp_</xsl:text><xsl:value-of select="@name"/><xsl:text>_rmv
  @</xsl:text><xsl:value-of select="attributes/attribute[position()=1]/@name"/><xsl:text> </xsl:text>
  <xsl:apply-templates select="attributes/attribute[position()=1]" mode="sql-type"/><xsl:text>,
  @vstamp INTEGER
AS
DELETE FROM </xsl:text><xsl:value-of select="@name"/><xsl:text>
WHERE (</xsl:text><xsl:value-of select="attributes/attribute[position()=1]/@name"/><xsl:text>=@</xsl:text><xsl:value-of select="attributes/attribute[position()=1]/@name"/><xsl:text>) AND (vstamp=@vstamp)
GO
</xsl:text>
  <xsl:if test="$db-platform/parameters/parameter[@name='grant-to']!=''">
    <xsl:text>GRANT EXECUTE ON sp_</xsl:text><xsl:value-of select="@name"/><xsl:text>_rmv TO </xsl:text><xsl:value-of select="$db-platform/parameters/parameter[@name='grant-to']"/><xsl:text>
GO
</xsl:text>
  </xsl:if>

</xsl:template>



</xsl:stylesheet>
