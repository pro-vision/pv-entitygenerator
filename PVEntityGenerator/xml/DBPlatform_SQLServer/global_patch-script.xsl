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

  Patch script
  DB Platform:  SQL Server

**************************************************************************************/
</xsl:text>

<xsl:for-each select="$project-settings/entity-export/patch-entities/patch-entity">
  <xsl:sort select="@patch-type"/>
  <xsl:sort select="@entity"/>
   <xsl:choose>
    <xsl:when test="@patch-type='DROP'">
      <xsl:apply-templates select="." mode="drop"/>
    </xsl:when>
    <xsl:when test="@patch-type='CREATE'">
      <xsl:apply-templates select="$db-definition/entities/entity[@name=current()/@entity]" mode="create"/>
    </xsl:when>
    <xsl:when test="@patch-type='MODIFY'">
      <xsl:apply-templates select="." mode="modify"/>
    </xsl:when>
  </xsl:choose>
</xsl:for-each>

</xsl:template>

<!-- modify entity -->
<xsl:template match="patch-entity" mode="modify">
  <xsl:variable name="entity" select="@entity"/>

  <!-- drop column -->
  <xsl:if test="patch-attribute[@patch-type='DROP']">
    <xsl:text>ALTER TABLE </xsl:text><xsl:value-of select="@entity"/><xsl:text> DROP COLUMN </xsl:text>
    <xsl:for-each select="patch-attribute[@patch-type='DROP']">
      <xsl:if test="position() &gt; 1">
        <xsl:text>,</xsl:text>
      </xsl:if>
      <xsl:value-of select="@name"/>
    </xsl:for-each>
  <xsl:text>
GO
</xsl:text>
  </xsl:if>
  <!-- create column -->
  <xsl:if test="patch-attribute[@patch-type='CREATE']">
    <xsl:text>ALTER TABLE </xsl:text><xsl:value-of select="@entity"/><xsl:text> ADD
</xsl:text>
    <xsl:for-each select="patch-attribute[@patch-type='CREATE']">
      <xsl:variable name="attr" select="$db-definition/entities/entity[@name=$entity]/attributes/attribute[@name=current()/@name]"/>
      <xsl:if test="$attr">
        <xsl:if test="position() &gt; 1">
      <xsl:text>,
</xsl:text>
        </xsl:if>
        <xsl:apply-templates select="$attr" mode="create"/>
      </xsl:if>

    </xsl:for-each>
    <xsl:text>
</xsl:text>


    <!-- possible primary keys -->
    <xsl:for-each select="patch-attribute[@patch-type='CREATE']">
      <xsl:variable name="pkey" select="$db-definition/entities/entity[@name=$entity]/keys/primary-key[attribute-ref/@attribute=current()/@name]"/>
      <xsl:if test="$pkey">
        <xsl:text>ALTER TABLE </xsl:text><xsl:value-of select="$entity"/><xsl:text> ADD </xsl:text>
        <xsl:apply-templates select="$pkey" mode="create">
          <xsl:with-param name="comma-on" select="'false'"/>
        </xsl:apply-templates>
        <xsl:text>
GO
</xsl:text>
    </xsl:if>

  </xsl:for-each>

    <!-- possible foreign keys -->
    <xsl:for-each select="patch-attribute[@patch-type='CREATE']">
      <xsl:variable name="fkey" select="$db-definition/entities/entity[@name=$entity]/keys/foreign-key[attribute-ref/@attribute=current()/@name]"/>
      <xsl:if test="$fkey">
        <xsl:text>ALTER TABLE </xsl:text><xsl:value-of select="$entity"/><xsl:text> ADD </xsl:text>
        <xsl:apply-templates select="$fkey" mode="create">
          <xsl:with-param name="comma-on" select="'false'"/>
        </xsl:apply-templates>
        <xsl:text>
GO
</xsl:text>
    </xsl:if>

  </xsl:for-each>


    <!-- possible indexes -->
    <xsl:for-each select="patch-attribute[@patch-type='CREATE']">
      <xsl:variable name="indexes" select="$db-definition/entities/entity[@name=$entity]/indexes/index[attribute-ref/@attribute=current()/@name]"/>
      <xsl:apply-templates select="$indexes" mode="create-index"/>
      <xsl:text>GO
</xsl:text>
    </xsl:for-each>

  </xsl:if>

   <!-- modify column -->
  <xsl:if test="patch-attribute[@patch-type='MODIFY']">
    <xsl:for-each select="patch-attribute[@patch-type='MODIFY']">
    <xsl:variable name="mod" select="$db-definition/entities/entity[@name=$entity]/attributes/attribute[@name=current()/@name]"/>
    <xsl:if test="$mod">
    <xsl:text>ALTER TABLE </xsl:text><xsl:value-of select="$entity"/><xsl:text> ALTER COLUMN
</xsl:text>
      <xsl:apply-templates select="$mod" mode="create"/>
      <xsl:text>
GO
</xsl:text>
  </xsl:if>
    </xsl:for-each>
  </xsl:if>

</xsl:template>

</xsl:stylesheet>
