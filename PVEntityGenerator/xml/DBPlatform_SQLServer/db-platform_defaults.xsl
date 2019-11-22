<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  version="1.0"
>


<!-- Globale Variablen -->
<xsl:variable name="db-definition" select="/pventitygenerator-data/db-definition"/>
<xsl:variable name="project-settings" select="/pventitygenerator-data/project-settings"/>
<xsl:variable name="db-platform" select="$project-settings/db-platforms/db-platform[@name='SQLServer']"/>

<!-- Globale keys -->
<xsl:key name="generate-entity" match="/pventitygenerator-data/project-settings/entity-generation/generate-entity" use="@entity"/>


<!-- Gibt einen SQL-Type zum Datentyp zurück -->
<xsl:template match="attribute" mode="sql-type">
  <xsl:choose>
    <xsl:when test="@type='VARCHAR'">
      <xsl:choose>
        <xsl:when test="$db-platform/parameters/parameter[@name='unicode']='true'">
          <xsl:text>NVARCHAR(</xsl:text><xsl:value-of select="@size"/><xsl:text>)</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>VARCHAR(</xsl:text><xsl:value-of select="@size"/><xsl:text>)</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:when>
    <xsl:when test="@type='SMALLINT'">
      <xsl:text>SMALLINT</xsl:text>
    </xsl:when>
    <xsl:when test="@type='INTEGER' or @type='ID' or @type='VSTAMP'">
      <xsl:text>INTEGER</xsl:text>
    </xsl:when>
    <xsl:when test="@type='BIGINT'">
      <xsl:text>BIGINT</xsl:text>
    </xsl:when>
    <xsl:when test="@type='FLOAT'">
      <xsl:text>FLOAT</xsl:text>
    </xsl:when>
    <xsl:when test="@type='DECIMAL'">
      <xsl:text>DECIMAL(</xsl:text><xsl:value-of select="@size"/><xsl:text>,</xsl:text><xsl:value-of select="@scale"/><xsl:text>)</xsl:text>
    </xsl:when>
    <xsl:when test="@type='BIT'">
      <xsl:text>BIT</xsl:text>
    </xsl:when>
    <xsl:when test="@type='TIMESTAMP'">
      <xsl:text>DATETIME</xsl:text>
    </xsl:when>
    <xsl:when test="@type='CLOB'">
      <xsl:choose>
        <xsl:when test="$db-platform/parameters/parameter[@name='unicode']='true'">
          <xsl:text>NTEXT</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>TEXT</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:when>
    <xsl:when test="@type='BLOB'">
      <xsl:text>IMAGE</xsl:text>
    </xsl:when>
  </xsl:choose>
</xsl:template>


<!-- Formatiert einen Default-Value abh. vom Datentype -->
<xsl:template name="format-value">
  <xsl:param name="value"/>
  <xsl:param name="type"/>

  <xsl:choose>
    <xsl:when test="$type='VARCHAR' or $type='CLOB'">
      <xsl:choose>
        <xsl:when test="$db-platform/parameters/parameter[@name='unicode']='true'">
          <xsl:text>N'</xsl:text><xsl:value-of select="$value"/><xsl:text>'</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>'</xsl:text><xsl:value-of select="$value"/><xsl:text>'</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:when>
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
      <xsl:text>{ts '</xsl:text>
      <xsl:value-of select="translate($value,'T',' ')"/>
      <xsl:text>'}</xsl:text>
    </xsl:when>
  </xsl:choose>
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
)
GO
</xsl:text>

  <xsl:if test="indexes/index">
  <xsl:apply-templates select="indexes/index" mode="create-index"/>
  <xsl:text>GO
</xsl:text>
  </xsl:if>

  <xsl:if test="$db-platform/parameters/parameter[@name='grant-to']!=''">
  <xsl:text>GRANT ALL ON </xsl:text><xsl:value-of select="@name"/><xsl:text> TO </xsl:text><xsl:value-of select="$db-platform/parameters/parameter[@name='grant-to']"/><xsl:text>
GO
</xsl:text>
  </xsl:if>

  <xsl:apply-templates select="attributes/attribute[@default-value!='' and @type!='BLOB']" mode="set-default"/>

</xsl:template>


<!-- Attribute definition -->
<xsl:template match="attribute" mode="create">
  <xsl:text>  </xsl:text>
  <xsl:value-of select="@name"/>
  <xsl:text> </xsl:text>
  <xsl:apply-templates select="." mode="sql-type"/>
  <xsl:if test="@auto-increment='true'">
    <xsl:text> IDENTITY</xsl:text>
  </xsl:if>
  <xsl:if test="@required='true'">
    <xsl:text> NOT</xsl:text>
  </xsl:if>
  <xsl:text> NULL</xsl:text>
</xsl:template>

<xsl:template match="attribute" mode="set-default">
  <xsl:text>CREATE DEFAULT </xsl:text><xsl:value-of select="concat(../../@name,'_df',position()-1)"/>
  <xsl:text> AS </xsl:text>
  <xsl:call-template name="format-value">
    <xsl:with-param name="value" select="@default-value"/>
    <xsl:with-param name="type" select="@type"/>
  </xsl:call-template>
  <xsl:text>
GO
sp_bindefault </xsl:text><xsl:value-of select="concat(../../@name,'_df',position()-1)"/>
  <xsl:text>, '</xsl:text><xsl:value-of select="concat(../../@name,'.',@name)"/>
  <xsl:text>'
GO
</xsl:text>
</xsl:template>

<!-- Primary/Foreign/Unique keys -->
<xsl:template match="primary-key" mode="create">
  <xsl:param name="comma-on"/>

  <xsl:if test="$comma-on='true'">
    <xsl:text>,
  </xsl:text>
  </xsl:if>

  <xsl:text>CONSTRAINT </xsl:text><xsl:value-of select="@name"/><xsl:text> PRIMARY KEY NONCLUSTERED (</xsl:text>
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
</xsl:template>

<xsl:template match="index" mode="create-index">
  <xsl:text>CREATE </xsl:text>
  <xsl:if test="@unique='true' and not($db-platform/parameters/parameter[@name='ignore-unique-indexes']='true')">
    <xsl:text>UNIQUE </xsl:text>
  </xsl:if>
  <xsl:text>INDEX </xsl:text><xsl:value-of select="@name"/>
  <xsl:text> ON </xsl:text><xsl:value-of select="../../@name"/><xsl:text>(</xsl:text>
  <xsl:apply-templates select="attribute-ref/@attribute" mode="attribute-list"/><xsl:text>)
</xsl:text>
</xsl:template>

<!-- Attributnamen-Auflstung durch "," getrennt -->
<xsl:template match="@*" mode="attribute-list">
  <xsl:if test="position() &gt; 1">, </xsl:if>
  <xsl:value-of select="."/>
</xsl:template>


<!-- Drop table -->
<xsl:template match="entity" mode="drop">
  <xsl:text>if exists (select * from sysobjects where id = object_id('</xsl:text><xsl:value-of select="@name"/><xsl:text>') and xtype='U')
  DROP TABLE </xsl:text><xsl:value-of select="@name"/><xsl:text>
GO
</xsl:text>
  <xsl:apply-templates select="$db-definition/entities/entity[@name=current()/@name]/attributes/attribute[@default-value!='' and @type!='BLOB']" mode="drop-default"/>
</xsl:template>
<xsl:template match="export-entity|patch-entity" mode="drop">
  <xsl:text>if exists (select * from sysobjects where id = object_id('</xsl:text><xsl:value-of select="@entity"/><xsl:text>') and xtype='U')
  DROP TABLE </xsl:text><xsl:value-of select="@entity"/><xsl:text>
GO
</xsl:text>
  <xsl:apply-templates select="$db-definition/entities/entity[@name=current()/@entity]/attributes/attribute[@default-value!='' and @type!='BLOB']" mode="drop-default"/>
</xsl:template>


<!-- Attribute Defaults -->
<xsl:template match="attribute" mode="drop-default">
  <xsl:text>if exists (select * from sysobjects where id = object_id('</xsl:text><xsl:value-of select="concat(../../@name,'_df',position()-1)"/><xsl:text>') and xtype='D')
  DROP DEFAULT </xsl:text><xsl:value-of select="concat(../../@name,'_df',position()-1)"/><xsl:text>
GO
</xsl:text>
</xsl:template>

</xsl:stylesheet>
