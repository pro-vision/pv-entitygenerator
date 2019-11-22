<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  version="1.0"
>


<xsl:param name="entity"/>


<!-- Globale Variablen -->
<xsl:variable name="db-definition" select="/pventitygenerator-data/db-definition"/>
<xsl:variable name="project-settings" select="/pventitygenerator-data/project-settings"/>
<xsl:variable name="platform" select="$project-settings/platforms/platform[@name='Java5']"/>

<xsl:variable name="generate-comments" select="$platform/parameters/parameter[@name='generate-comments']='true'"/>

<xsl:variable name="entity-class" select="concat($platform/parameters/parameter[@name='entity-package'],'.',$entity)"/>
<xsl:variable name="generate-entity" select="key('generate-entity', $entity)"/>
<xsl:variable name="generate-entity-platform" select="key('generate-entity-platform', $entity)"/>


<!-- Globale keys -->
<xsl:key name="generate-entity" match="/pventitygenerator-data/project-settings/entity-generation/generate-entity" use="@entity"/>
<xsl:key name="generate-entity-platform" match="/pventitygenerator-data/project-settings/platforms/platform[@name='Java5']/entity-generation/generate-entity" use="@entity"/>

<xsl:key name="foreign-key-check" match="/pventitygenerator-data/db-definition/entities/entity/keys/foreign-key[@cascading-delete='false']" use="@foreign-entity"/>
<xsl:key name="foreign-key-cascading-delete" match="/pventitygenerator-data/db-definition/entities/entity/keys/foreign-key[@cascading-delete='true']" use="@foreign-entity"/>


<!-- Test values -->
<xsl:variable name="testvalue-chars100" select="'üüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüü'"/>
<xsl:variable name="testvalue-chars1000" select="concat($testvalue-chars100,$testvalue-chars100,$testvalue-chars100,$testvalue-chars100,$testvalue-chars100,$testvalue-chars100,$testvalue-chars100,$testvalue-chars100,$testvalue-chars100,$testvalue-chars100)"/>
<xsl:variable name="testvalue-chars10000" select="concat($testvalue-chars1000,$testvalue-chars1000,$testvalue-chars1000,$testvalue-chars1000,$testvalue-chars1000,$testvalue-chars1000,$testvalue-chars1000,$testvalue-chars1000,$testvalue-chars1000,$testvalue-chars1000)"/>


<!-- Formatiert einen Wert für Java-Code abhängig vom Typ -->
<xsl:template name="format-type-value">
  <xsl:param name="type"/>
  <xsl:param name="value"/>

  <xsl:choose>

    <xsl:when test="$type='VARCHAR' or $type='CLOB'">
      <xsl:text>&quot;</xsl:text>
      <xsl:call-template name="replace-string">
        <xsl:with-param name="text" select="$value"/>
        <xsl:with-param name="replace">&quot;</xsl:with-param>
        <xsl:with-param name="with">\&quot;</xsl:with-param>
      </xsl:call-template>
      <xsl:text>&quot;</xsl:text>
    </xsl:when>

    <xsl:when test="$type='INTEGER' or $type='SMALLINT' or $type='ID' or $type='VSTAMP'">
      <xsl:value-of select="$value"/>
    </xsl:when>

    <xsl:when test="$type='BIGINT'">
      <xsl:value-of select="$value"/>
      <xsl:text>l</xsl:text>
    </xsl:when>

    <xsl:when test="$type='FLOAT' or $type='DECIMAL'">
      <xsl:call-template name="replace-string">
        <xsl:with-param name="text" select="$value"/>
        <xsl:with-param name="replace">,</xsl:with-param>
        <xsl:with-param name="with">.</xsl:with-param>
      </xsl:call-template>
      <xsl:text>d</xsl:text>
    </xsl:when>

    <xsl:when test="$type='BIT'">
      <xsl:choose>
        <xsl:when test="$value='1' or $value='true'"><xsl:text>true</xsl:text></xsl:when>
        <xsl:otherwise><xsl:text>false</xsl:text></xsl:otherwise>
      </xsl:choose>
    </xsl:when>

    <xsl:when test="$type='TIMESTAMP'">
      <xsl:text>de.provision.framework.util.PVFormatUtil.parseDateTimeXml(&quot;</xsl:text>
      <xsl:value-of select="$value"/>
      <xsl:text>&quot;)</xsl:text>
    </xsl:when>

    <xsl:when test="$type='BLOB'">
      <xsl:text>null</xsl:text>
    </xsl:when>

  </xsl:choose>
</xsl:template>


<!-- Gibt einen Java-Type zum Datentyp zurück -->
<xsl:template name="variable-type">
  <xsl:param name="type"/>
  <xsl:choose>
    <xsl:when test="$type='VARCHAR' or $type='CLOB'">
      <xsl:text>String</xsl:text>
    </xsl:when>
    <xsl:when test="$type='INTEGER' or $type='SMALLINT' or $type='ID' or $type='VSTAMP'">
      <xsl:text>int</xsl:text>
    </xsl:when>
    <xsl:when test="$type='BIGINT'">
      <xsl:text>long</xsl:text>
    </xsl:when>
    <xsl:when test="$type='FLOAT' or $type='DECIMAL'">
      <xsl:text>double</xsl:text>
    </xsl:when>
    <xsl:when test="$type='BIT'">
      <xsl:text>boolean</xsl:text>
    </xsl:when>
    <xsl:when test="$type='TIMESTAMP'">
      <xsl:text>java.sql.Timestamp</xsl:text>
    </xsl:when>
    <xsl:when test="$type='BLOB'">
      <xsl:text>byte[]</xsl:text>
    </xsl:when>
  </xsl:choose>
</xsl:template>


<!-- Gibt den Datentyp zum Method Attribute zurück -->
<xsl:template match="method-attribute" mode="type">
  <xsl:variable name="name" select="@name"/>
  <xsl:variable name="entity" select="../../../@entity"/>

  <xsl:choose>
    <xsl:when test="@type!=''">
      <xsl:value-of select="@type"/>
    </xsl:when>
    <xsl:otherwise>
      <xsl:value-of select="/pventitygenerator-data/db-definition/entities/entity[@name=$entity]/attributes/attribute[@name=$name]/@type"/>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>


<!-- Gibt einen Enumerations-Type zu einem attribute zurück -->
<xsl:template name="variable-enum-type">
  <xsl:param name="attribute"/>

  <xsl:variable name="current-entity" select="$attribute/ancestor::*[2]"/>

  <!-- Bei ID-attributes prüfen, ob es sich um eine Referenz auf ein Enumeration-Entity handelt -->
  <xsl:if test="$attribute/@type='ID'">
    <xsl:variable name="foreign-key" select="$current-entity/keys/foreign-key[attribute-ref/@attribute=$attribute/@name]"/>
    <xsl:if test="$foreign-key">
      <xsl:variable name="foreign-entity" select="$db-definition/entities/entity[@name=$foreign-key/@foreign-entity]"/>
      <xsl:if test="$foreign-entity">

        <!-- Das ganze aber nur, wenn wirklich für alle Items Konstanten generiert werden... -->
        <xsl:variable name="generate-foreign-entity" select="key('generate-entity', $foreign-entity/@name)"/>
        <xsl:if test="($generate-foreign-entity/parameters/parameter[@name='generate-enumeration']='true')
            and not($generate-foreign-entity/enumeration-entries/enumeration-entry[@generate='0'])">
          <xsl:value-of select="$foreign-entity/@name"/>
        </xsl:if>
      </xsl:if>
    </xsl:if>
  </xsl:if>

</xsl:template>


<!-- Gibt den Datentyp zum Method Attribute zurück -->
<xsl:template match="method-attribute" mode="enum-type">
  <xsl:variable name="name" select="@name"/>
  <xsl:variable name="entity" select="../../../@entity"/>

  <xsl:if test="@type='' or not (@type)">
    <xsl:call-template name="variable-enum-type">
      <xsl:with-param name="attribute" select="/pventitygenerator-data/db-definition/entities/entity[@name=$entity]/attributes/attribute[@name=$name]"/>
      <xsl:with-param name="entity" select="/pventitygenerator-data/db-definition/entities/entity[@name=$entity]"/>
    </xsl:call-template>
  </xsl:if>
</xsl:template>


<!-- Gibt eine Java-Type-Bezeichnung für get/set-Methodenaufrufe zum Datentyp zurück -->
<xsl:template name="method-type">
  <xsl:param name="type"/>
  <xsl:choose>
    <xsl:when test="$type='VARCHAR' or $type='CLOB'">
      <xsl:text>String</xsl:text>
    </xsl:when>
    <xsl:when test="$type='INTEGER' or $type='SMALLINT' or $type='ID' or $type='VSTAMP'">
      <xsl:text>Int</xsl:text>
    </xsl:when>
    <xsl:when test="$type='BIGINT'">
      <xsl:text>Long</xsl:text>
    </xsl:when>
    <xsl:when test="$type='FLOAT' or $type='DECIMAL'">
      <xsl:text>Double</xsl:text>
    </xsl:when>
    <xsl:when test="$type='BIT'">
      <xsl:text>Boolean</xsl:text>
    </xsl:when>
    <xsl:when test="$type='TIMESTAMP'">
      <xsl:text>Timestamp</xsl:text>
    </xsl:when>
    <xsl:when test="$type='BLOB'">
      <xsl:text>Bytes</xsl:text>
    </xsl:when>
  </xsl:choose>
</xsl:template>


<!-- Gibt einen Java-Type zum Datentyp zurück -->
<xsl:template name="type-prefix">
  <xsl:param name="type"/>
  <!-- ** type prefixes removed
  <xsl:choose>
    <xsl:when test="$type='VARCHAR' or $type='CLOB'">
      <xsl:text>str</xsl:text>
    </xsl:when>
    <xsl:when test="$type='INTEGER' or $type='SMALLINT' or $type='ID' or $type='VSTAMP'">
      <xsl:text>int</xsl:text>
    </xsl:when>
    <xsl:when test="$type='BIGINT'">
      <xsl:text>lng</xsl:text>
    </xsl:when>
    <xsl:when test="$type='FLOAT' or $type='DECIMAL'">
      <xsl:text>dbl</xsl:text>
    </xsl:when>
    <xsl:when test="$type='BIT'">
      <xsl:text>f</xsl:text>
    </xsl:when>
    <xsl:when test="$type='TIMESTAMP'">
      <xsl:text>dat</xsl:text>
    </xsl:when>
    <xsl:when test="$type='BLOB'">
      <xsl:text>abyt</xsl:text>
    </xsl:when>
  </xsl:choose>
  -->
</xsl:template>


<!-- Gibt eine Java-Type-Bezeichnung für get/set-Methodenaufrufe zum Datentyp zurück -->
<xsl:template name="test-test-value">
  <xsl:param name="attribute"/>
  <xsl:param name="type"/>
  <xsl:param name="force-required" select="0"/>
  <xsl:param name="check-enum" select="1"/>

  <xsl:variable name="value-type">
    <xsl:choose>
      <xsl:when test="$type!=''"><xsl:value-of select="$type"/></xsl:when>
      <xsl:otherwise><xsl:value-of select="$attribute/@type"/></xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="enum-type">
    <xsl:call-template name="variable-enum-type">
      <xsl:with-param name="attribute" select="$attribute"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:choose>
    <xsl:when test="$enum-type!='' and $check-enum=1">
      <xsl:variable name="generate-foreign-entity" select="key('generate-entity', $enum-type)"/>
      <xsl:choose>
        <xsl:when test="$generate-foreign-entity/enumeration-entries/enumeration-entry[1]/@identifier!=''">
          <xsl:value-of select="concat($platform/parameters/parameter[@name='entity-package'],'.',$enum-type,'.')"/>
          <xsl:value-of select="$generate-foreign-entity/enumeration-entries/enumeration-entry[1]/@identifier"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>null</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:when>
    <xsl:when test="$value-type='VARCHAR'">
      <xsl:variable name="size">
        <xsl:choose>
          <xsl:when test="$attribute"><xsl:value-of select="$attribute/@size"/></xsl:when>
          <xsl:otherwise>10</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:value-of select="concat('&quot;',substring($testvalue-chars10000,1,$size),'&quot;')"/>
    </xsl:when>
    <xsl:when test="$value-type='SMALLINT'">
      <xsl:text>12345</xsl:text>
    </xsl:when>
    <xsl:when test="$value-type='INTEGER'">
      <xsl:text>1234567890</xsl:text>
    </xsl:when>
    <xsl:when test="$value-type='BIGINT'">
      <xsl:text>123456789012345678l</xsl:text>
    </xsl:when>
    <xsl:when test="$value-type='FLOAT' or $value-type='DECIMAL'">
      <xsl:text>123456789.1234567d</xsl:text>
    </xsl:when>
    <xsl:when test="$value-type='BIT'">
      <xsl:text>true</xsl:text>
    </xsl:when>
    <xsl:when test="$value-type='TIMESTAMP'">
      <xsl:text>de.provision.framework.util.PVFormatUtil.parseDateTimeXml("2001-03-15T16:20:15")</xsl:text>
    </xsl:when>
    <xsl:when test="$value-type='CLOB'">
      <xsl:text>"Der Jodelkaiser\naus dem Ötztal\nist wieder daheim!"</xsl:text>
    </xsl:when>
    <xsl:when test="$value-type='BLOB'">
      <xsl:text>null</xsl:text>
    </xsl:when>
    <xsl:when test="$value-type='ID' or $value-type='VSTAMP'">
      <xsl:choose>
        <xsl:when test="$attribute/@required='true' or $force-required=1">1</xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:when>
  </xsl:choose>
</xsl:template>


<!-- Gibt eine Java-Type-Bezeichnung für get/set-Methodenaufrufe zum Datentyp zurück -->
<xsl:template name="test-default-value">
  <xsl:param name="attribute"/>

  <xsl:variable name="enum-type">
    <xsl:call-template name="variable-enum-type">
      <xsl:with-param name="attribute" select="$attribute"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:choose>
    <xsl:when test="$enum-type!=''">
      <xsl:variable name="value">
        <xsl:call-template name="replace-string">
          <xsl:with-param name="text" select="$attribute/@default-value"/>
          <xsl:with-param name="replace" select="','"/>
          <xsl:with-param name="with" select="'.'"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:variable name="generate-foreign-entity" select="key('generate-entity', $enum-type)"/>
      <xsl:choose>
        <xsl:when test="$generate-foreign-entity/enumeration-entries/enumeration-entry[@id=$value]/@identifier!=''">
          <xsl:value-of select="$enum-type"/>
          <xsl:text>.</xsl:text>
          <xsl:value-of select="$generate-foreign-entity/enumeration-entries/enumeration-entry[@id=$value]/@identifier"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>null</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:when>
    <xsl:when test="$attribute/@type='VARCHAR' or $attribute/@type='CLOB'">
      <xsl:variable name="value">
        <xsl:call-template name="replace-string">
          <xsl:with-param name="text" select="$attribute/@default-value"/>
          <xsl:with-param name="replace" select="'&quot;'"/>
          <xsl:with-param name="with" select="'\&quot;'"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:value-of select="concat('&quot;',$value,'&quot;')"/>
    </xsl:when>
    <xsl:when test="$attribute/@type='SMALLINT' or $attribute/@type='INTEGER' or $attribute/@type='ID' or $attribute/@type='VSTAMP'">
      <xsl:variable name="value">
        <xsl:call-template name="replace-string">
          <xsl:with-param name="text" select="$attribute/@default-value"/>
          <xsl:with-param name="replace" select="','"/>
          <xsl:with-param name="with" select="'.'"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="$value!=''">
          <xsl:value-of select="$value"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>0</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:when>
    <xsl:when test="$attribute/@type='BIGINT'">
      <xsl:variable name="value">
        <xsl:call-template name="replace-string">
          <xsl:with-param name="text" select="$attribute/@default-value"/>
          <xsl:with-param name="replace" select="','"/>
          <xsl:with-param name="with" select="'.'"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="$value!=''">
          <xsl:value-of select="concat($value,'l')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>0l</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:when>
    <xsl:when test="$attribute/@type='FLOAT' or $attribute/@type='DECIMAL'">
      <xsl:variable name="value">
        <xsl:call-template name="replace-string">
          <xsl:with-param name="text" select="$attribute/@default-value"/>
          <xsl:with-param name="replace" select="','"/>
          <xsl:with-param name="with" select="'.'"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="$value!=''">
          <xsl:value-of select="concat($value,'d')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>0d</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:when>
    <xsl:when test="$attribute/@type='BIT'">
      <xsl:choose>
        <xsl:when test="$attribute/@default-value=1 or $attribute/@default-value='true'">
          <xsl:text>true</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:when>
    <xsl:when test="$attribute/@type='TIMESTAMP'">
      <xsl:variable name="value">
        <xsl:call-template name="replace-string">
          <xsl:with-param name="text" select="$attribute/@default-value"/>
          <xsl:with-param name="replace" select="','"/>
          <xsl:with-param name="with" select="'.'"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="$value!=''">
          <xsl:text>de.provision.framework.util.PVFormatUtil.parseDateTimeXml("</xsl:text><xsl:value-of select="$value"/><xsl:text>")</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>null</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:when>
    <xsl:when test="$attribute/@type='BLOB'">
      <xsl:text>null</xsl:text>
    </xsl:when>
  </xsl:choose>
</xsl:template>


<!-- XSLT-Replace-Funktion *** -->
<xsl:template name="replace-string">
  <xsl:param name="text"/>
  <xsl:param name="replace"/>
  <xsl:param name="with"/>
  <xsl:param name="replace-2"/>
  <xsl:param name="with-2"/>
  <xsl:param name="replace-3"/>
  <xsl:param name="with-3"/>
  <xsl:param name="replace-4"/>
  <xsl:param name="with-4"/>

  <xsl:variable name="result">
    <xsl:choose>
      <xsl:when test="string-length($replace) = 0">
        <xsl:value-of select="$text"/>
      </xsl:when>
      <xsl:when test="contains($text, $replace)">
        <xsl:variable name="before" select="substring-before($text, $replace)"/>
        <xsl:variable name="after" select="substring-after($text, $replace)"/>
        <xsl:value-of select="$before"/>
        <xsl:value-of select="$with"/>
        <xsl:call-template name="replace-string">
          <xsl:with-param name="text" select="$after"/>
          <xsl:with-param name="replace" select="$replace"/>
          <xsl:with-param name="with" select="$with"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$text"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:choose>
    <xsl:when test="$replace-2!=''">
      <xsl:call-template name="replace-string">
        <xsl:with-param name="text" select="$result"/>
        <xsl:with-param name="replace" select="$replace-2"/>
        <xsl:with-param name="with" select="$with-2"/>
        <xsl:with-param name="replace-2" select="$replace-3"/>
        <xsl:with-param name="with-2" select="$with-3"/>
        <xsl:with-param name="replace-3" select="$replace-4"/>
        <xsl:with-param name="with-3" select="$with-4"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:otherwise>
      <xsl:value-of select="$result"/>
    </xsl:otherwise>
  </xsl:choose>

</xsl:template>


</xsl:stylesheet>
