<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  version="1.0"
>

<!-- Globale Variablen -->
<xsl:variable name="db-definition" select="/pventitygenerator-data/db-definition"/>
<xsl:variable name="project-settings" select="/pventitygenerator-data/project-settings"/>
<xsl:variable name="db-platform" select="$project-settings/db-platforms/db-platform[@name='MySQL']"/>

<!-- Globale keys -->
<xsl:key name="generate-entity" match="/pventitygenerator-data/project-settings/entity-generation/generate-entity" use="@entity"/>

<!-- Gibt einen SQL-Type zum Datentyp zurück -->
<xsl:template match="attribute" mode="sql-type">
  <xsl:choose>
    <xsl:when test="@type='VARCHAR'">
      <xsl:text>VARCHAR(</xsl:text><xsl:value-of select="@size"/><xsl:text>)</xsl:text>
    </xsl:when>
    <xsl:when test="@type='SMALLINT'">
      <xsl:text>SMALLINT</xsl:text>
    </xsl:when>
    <xsl:when test="@type='INTEGER' or @type='ID' or @type='VSTAMP'">
      <xsl:text>INT</xsl:text>
    </xsl:when>
    <xsl:when test="@type='BIGINT'">
      <xsl:text>BIGINT</xsl:text>
    </xsl:when>
    <xsl:when test="@type='FLOAT'">
      <xsl:text>DOUBLE</xsl:text>
    </xsl:when>
    <xsl:when test="@type='DECIMAL'">
      <xsl:text>DECIMAL(</xsl:text><xsl:value-of select="@size"/><xsl:text>,</xsl:text><xsl:value-of select="@scale"/><xsl:text>)</xsl:text>
    </xsl:when>
    <xsl:when test="@type='BIT'">
      <xsl:text>SMALLINT</xsl:text>
    </xsl:when>
    <xsl:when test="@type='TIMESTAMP'">
      <xsl:text>DATETIME</xsl:text>
    </xsl:when>
    <xsl:when test="@type='CLOB'">
      <xsl:text>LONGTEXT</xsl:text>
    </xsl:when>
    <xsl:when test="@type='BLOB'">
      <xsl:text>LONGBLOB</xsl:text>
    </xsl:when>
  </xsl:choose>
</xsl:template>


<!-- Formatiert einen Default-Value abh. vom Datentype -->
<xsl:template name="format-value">
  <xsl:param name="value"/>
  <xsl:param name="type"/>

  <xsl:choose>
    <!-- Keine Default-Werte für TEXT möglich, also auch für VARCHAR erstmal ausblenden
      <xsl:when test="$type='VARCHAR' or $type='CLOB'">
      <xsl:text>'</xsl:text><xsl:value-of select="$value"/><xsl:text>'</xsl:text>
    </xsl:when>-->
    <xsl:when test="$type='INTEGER' or $type='ID' or $type='VSTAMP' or $type='SMALLINT' or $type='BIGINT'">
      <xsl:value-of select="$value"/>
    </xsl:when>
    <xsl:when test="$type='FLOAT' or $type='DECIMAL'">
      <xsl:value-of select="$value"/>
    </xsl:when>
    <xsl:when test="$type='BIT'">
      <xsl:choose>
        <xsl:when test="$value='True' or $value='true' or $value='Wahr' or $value='Yes' or $value='Ja' or $value='1'">1</xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:when>
    <xsl:when test="$type='TIMESTAMP'">
      <xsl:text>'</xsl:text>
      <xsl:value-of select="translate($value,'T',' ')"/>
      <xsl:text>'</xsl:text>
    </xsl:when>
  </xsl:choose>
</xsl:template>


<!-- Attributnamen-Auflistung durch "," getrennt -->
<xsl:template match="@*" mode="attribute-list">
  <xsl:if test="position() &gt; 1">, </xsl:if>
  <xsl:value-of select="."/>
</xsl:template>

<!-- Drop table -->
<xsl:template match="entity" mode="drop">
  <xsl:text>DROP TABLE IF EXISTS </xsl:text><xsl:value-of select="@name"/><xsl:text>;
</xsl:text>
</xsl:template>
<xsl:template match="export-entity|patch-entity" mode="drop">
  <xsl:text>DROP TABLE IF EXISTS </xsl:text><xsl:value-of select="@entity"/><xsl:text>;
</xsl:text>
</xsl:template>

<!-- Create table -->
<xsl:template match="entity" mode="create">
  <xsl:variable name="ent" select="."/>

  <xsl:text>
/*** Create table '</xsl:text><xsl:value-of select="@name"/><xsl:text>' ***/
CREATE TABLE </xsl:text><xsl:value-of select="@name"/><xsl:text> (
</xsl:text>

  <xsl:for-each select="attributes/attribute">
    <xsl:if test="position() &gt; 1">
      <xsl:text>,
</xsl:text>
    </xsl:if>
    <xsl:apply-templates select="." mode="create"/>
  </xsl:for-each>

  <xsl:apply-templates select="keys/primary-key" mode="create">
    <xsl:with-param name="comma-on" select="'true'"/>
  </xsl:apply-templates>
  <xsl:apply-templates select="keys/foreign-key[@foreign-entity!=$ent/@name or not($db-platform/parameters/parameter[@name='ignore-self-references']='true')]" mode="create">
    <xsl:with-param name="comma-on" select="'true'"/>
  </xsl:apply-templates>

  <xsl:text>
) ENGINE=InnoDB;
</xsl:text>

  <xsl:if test="indexes/index">
    <xsl:apply-templates select="indexes/index" mode="create-index"/>
  </xsl:if>

  <xsl:if test="$db-platform/parameters/parameter[@name='grant-to']!=''">
  <xsl:text>GRANT ALL ON </xsl:text><xsl:value-of select="@name"/><xsl:text> TO </xsl:text><xsl:value-of select="$db-platform/parameters/parameter[@name='grant-to']"/>;<xsl:text>

</xsl:text>
  </xsl:if>

</xsl:template>


<!-- Attribute definition -->
<xsl:template match="attribute" mode="create">
  <xsl:text>  </xsl:text>
  <xsl:value-of select="@name"/>
  <xsl:text> </xsl:text>
  <xsl:apply-templates select="." mode="sql-type"/>
  <xsl:if test="@default-value!='' and not(@type='VARCHAR') and not(@type='CLOB')">
    <xsl:text> DEFAULT </xsl:text>
    <xsl:call-template name="format-value">
      <xsl:with-param name="value" select="@default-value"/>
      <xsl:with-param name="type" select="@type"/>
    </xsl:call-template>
  </xsl:if>
  <xsl:if test="@required='true'">
    <xsl:text> NOT</xsl:text>
  </xsl:if>
  <xsl:text> NULL</xsl:text>
  <xsl:if test="@auto-increment='true'">
    <xsl:text> AUTO_INCREMENT</xsl:text>
  </xsl:if>
</xsl:template>


<!-- Primary/Foreign/Unique keys -->
<xsl:template match="primary-key" mode="create">
  <xsl:param name="comma-on"/>

  <xsl:if test="$comma-on='true'">
    <xsl:text>,
  </xsl:text>
  </xsl:if>

  <xsl:text>CONSTRAINT </xsl:text>

  <xsl:value-of select="@name"/><xsl:text> PRIMARY KEY (</xsl:text>
  <xsl:apply-templates select="attribute-ref/@attribute" mode="attribute-list"/><xsl:text>)</xsl:text>
</xsl:template>

<xsl:template match="foreign-key" mode="create">
  <xsl:param name="comma-on"/>

  <xsl:if test="$comma-on='true'">
    <xsl:text>,
  </xsl:text>
  </xsl:if>

  <xsl:text>CONSTRAINT </xsl:text><xsl:value-of select="@name"/>
  <xsl:text> FOREIGN KEY (</xsl:text>
  <xsl:apply-templates select="attribute-ref/@attribute" mode="attribute-list"/>
  <xsl:text>) REFERENCES </xsl:text><xsl:value-of select="@foreign-entity"/><xsl:text> (</xsl:text>
  <xsl:apply-templates select="attribute-ref/@foreign-attribute" mode="attribute-list"/><xsl:text>)</xsl:text>
  <!--ON UPDATE RESTRICT und ON DELETE RESTRICT pro ForeignKey sollten vielleicht noch rein-->
</xsl:template>

<xsl:template match="index" mode="create-index">
  <xsl:text>CREATE </xsl:text>
  <xsl:if test="@unique='true' and not($db-platform/parameters/parameter[@name='ignore-unique-indexes']='true')">
    <xsl:text>UNIQUE </xsl:text>
  </xsl:if>
  <xsl:text>INDEX </xsl:text><xsl:value-of select="@name"/>
  <xsl:text> ON </xsl:text><xsl:value-of select="../../@name"/><xsl:text>(</xsl:text>
  <xsl:apply-templates select="attribute-ref/@attribute" mode="attribute-list"/><xsl:text>);
</xsl:text>
</xsl:template>

</xsl:stylesheet>
