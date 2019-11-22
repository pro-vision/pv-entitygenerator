<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  version="1.0"
>


<!-- Globale Variablen -->
<xsl:variable name="db-definition" select="/pventitygenerator-data/db-definition"/>
<xsl:variable name="project-settings" select="/pventitygenerator-data/project-settings"/>
<xsl:variable name="db-platform" select="$project-settings/db-platforms/db-platform[@name='Oracle']"/>
<xsl:variable name="name-max-length" select="number($db-platform/parameters/parameter[@name='name-max-length'])"/>
<xsl:variable name="identity-strategy" select="$project-settings/parameters/parameter[@name='identity-strategy']"/>
<xsl:variable name="sequence-initial-value" select="$project-settings/parameters/parameter[@name='sequence-initial-value']"/>

<!-- Globale keys -->
<xsl:key name="generate-entity" match="/pventitygenerator-data/project-settings/entity-generation/generate-entity" use="@entity"/>


<!-- Gibt einen SQL-Type zum Datentyp zurück -->
<xsl:template match="attribute" mode="sql-type">
  <xsl:choose>
    <xsl:when test="@type='VARCHAR'">
      <xsl:choose>
        <xsl:when test="$db-platform/parameters/parameter[@name='unicode']='true'">
          <xsl:text>NVARCHAR2(</xsl:text><xsl:value-of select="@size"/>
          <!--
          <xsl:if test="$db-platform/parameters/parameter[@name='oracle-8i']!='true'">
            <xsl:text> CHAR</xsl:text>
          </xsl:if>-->
          <xsl:text>)</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>VARCHAR2(</xsl:text><xsl:value-of select="@size"/>
          <xsl:if test="$db-platform/parameters/parameter[@name='oracle-8i']!='true'">
            <xsl:text> CHAR</xsl:text>
          </xsl:if>
          <xsl:text>)</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:when>
    <xsl:when test="@type='SMALLINT'">
      <xsl:text>NUMBER(5)</xsl:text>
    </xsl:when>
    <xsl:when test="@type='INTEGER' or @type='ID' or @type='VSTAMP'">
      <xsl:text>NUMBER(10)</xsl:text>
    </xsl:when>
    <xsl:when test="@type='BIGINT'">
      <xsl:text>NUMBER(18,0)</xsl:text>
    </xsl:when>
    <xsl:when test="@type='FLOAT'">
      <xsl:text>FLOAT(64)</xsl:text>
    </xsl:when>
    <xsl:when test="@type='DECIMAL'">
      <xsl:text>NUMBER(</xsl:text><xsl:value-of select="@size"/><xsl:text>,</xsl:text><xsl:value-of select="@scale"/><xsl:text>)</xsl:text>
    </xsl:when>
    <xsl:when test="@type='BIT'">
      <xsl:text>NUMBER(2)</xsl:text>
    </xsl:when>
    <xsl:when test="@type='TIMESTAMP'">
      <xsl:text>DATE</xsl:text>
    </xsl:when>
    <xsl:when test="@type='CLOB'">
      <xsl:choose>
        <xsl:when test="$db-platform/parameters/parameter[@name='unicode']='true'">
          <xsl:text>NCLOB</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>CLOB</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:when>
    <xsl:when test="@type='BLOB'">
      <xsl:text>BLOB</xsl:text>
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
      <xsl:text>TO_DATE('</xsl:text>
      <xsl:value-of select="translate($value,'T',' ')"/>
      <xsl:text>','YYYY-MM-DD HH24:MI:SS')</xsl:text>
    </xsl:when>
  </xsl:choose>
</xsl:template>


<!-- Attributnamen-Auflstung durch "," getrennt -->
<xsl:template match="@*" mode="attribute-list">
  <xsl:if test="position() &gt; 1">, </xsl:if>
  <xsl:value-of select="."/>
</xsl:template>


<!-- Drop table -->
<xsl:template match="entity" mode="drop">
  <xsl:text>BEGIN
  EXECUTE IMMEDIATE 'DROP TABLE </xsl:text><xsl:value-of select="@name"/><xsl:text> CASCADE CONSTRAINTS';
  EXCEPTION
    WHEN OTHERS THEN COMMIT;
END;
/
</xsl:text>
</xsl:template>
<xsl:template match="export-entity|patch-entity" mode="drop">
  <xsl:text>BEGIN
  EXECUTE IMMEDIATE 'DROP TABLE </xsl:text><xsl:value-of select="@entity"/><xsl:text> CASCADE CONSTRAINTS';
  EXCEPTION
    WHEN OTHERS THEN COMMIT;
END;
/
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
)
/
</xsl:text>

  <xsl:if test="indexes/index">
    <xsl:apply-templates select="indexes/index" mode="create-index"/>
  </xsl:if>

  <xsl:if test="$db-platform/parameters/parameter[@name='grant-to']!=''">
  <xsl:text>GRANT ALL ON </xsl:text><xsl:value-of select="@name"/><xsl:text> TO </xsl:text><xsl:value-of select="$db-platform/parameters/parameter[@name='grant-to']"/><xsl:text>
/
</xsl:text>
  </xsl:if>

</xsl:template>


<!-- Drop sequence -->
<xsl:template match="entity" mode="drop-sequence">
  <xsl:text>BEGIN
  EXECUTE IMMEDIATE 'DROP SEQUENCE </xsl:text><xsl:value-of select="@name"/><xsl:text>_sq';
  EXCEPTION
    WHEN OTHERS THEN COMMIT;
END;
/
</xsl:text>
</xsl:template>
<xsl:template match="export-entity|patch-entity" mode="drop-sequence">
  <xsl:text>BEGIN
  EXECUTE IMMEDIATE 'DROP SEQUENCE </xsl:text><xsl:value-of select="@entity"/><xsl:text>_sq';
  EXCEPTION
    WHEN OTHERS THEN COMMIT;
END;
/
</xsl:text>
</xsl:template>


<!-- Create sequence -->
<xsl:template match="entity" mode="create-sequence">
  <xsl:variable name="ent" select="."/>

  <xsl:text>
/*** Create sequence '</xsl:text><xsl:value-of select="@name"/><xsl:text>_sq' ***/
CREATE SEQUENCE </xsl:text><xsl:value-of select="@name"/><xsl:text>_sq</xsl:text>
  <xsl:if test="$sequence-initial-value &gt; 0">
    <xsl:text> START WITH </xsl:text>
    <xsl:value-of select="$sequence-initial-value"/>
  </xsl:if>
<xsl:text>
/
</xsl:text>

</xsl:template>


<!-- Attribute definition -->
<xsl:template match="attribute" mode="create">
  <xsl:text>  </xsl:text>
  <xsl:value-of select="@name"/>
  <xsl:text> </xsl:text>
  <xsl:apply-templates select="." mode="sql-type"/>
  <xsl:if test="@default-value!=''">
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
</xsl:template>


<!-- Primary/Foreign/Unique keys -->
<xsl:template match="primary-key" mode="create">
  <xsl:param name="comma-on"/>

  <xsl:if test="$comma-on='true'">
    <xsl:text>,
  </xsl:text>
  </xsl:if>

  <xsl:text>CONSTRAINT </xsl:text>
  <xsl:call-template name="check-name-max-length">
    <xsl:with-param name="name" select="@name"/>
  </xsl:call-template>
  <xsl:text> PRIMARY KEY (</xsl:text>
  <xsl:apply-templates select="attribute-ref/@attribute" mode="attribute-list"/><xsl:text>)</xsl:text>
</xsl:template>

<xsl:template match="foreign-key" mode="create">
  <xsl:param name="comma-on"/>

  <xsl:if test="$comma-on='true'">
    <xsl:text>,
  </xsl:text>
  </xsl:if>

  <xsl:text>CONSTRAINT </xsl:text>
  <xsl:call-template name="check-name-max-length">
    <xsl:with-param name="name" select="@name"/>
  </xsl:call-template>
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
  <xsl:text>INDEX </xsl:text>
  <xsl:call-template name="check-name-max-length">
    <xsl:with-param name="name" select="@name"/>
  </xsl:call-template>
  <xsl:text> ON </xsl:text><xsl:value-of select="../../@name"/><xsl:text>(</xsl:text>
  <xsl:apply-templates select="attribute-ref/@attribute" mode="attribute-list"/><xsl:text>)
/
</xsl:text>
</xsl:template>


<!-- special max length check template for oracle object names -->
<xsl:template name="check-name-max-length">
  <xsl:param name="name"/>
  <xsl:choose>
    <xsl:when test="string-length($name) &gt; $name-max-length">
      <xsl:value-of select="substring($name, string-length($name) - $name-max-length + 1)"/>
    </xsl:when>
    <xsl:otherwise>
      <xsl:value-of select="$name"/>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>


</xsl:stylesheet>
