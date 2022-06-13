<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet
  xmlns="http://java.sun.com/xml/ns/persistence/orm" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  version="1.0"
>

<xsl:import href="platform_defaults.xsl"/>

<xsl:output method="xml" encoding="UTF-8" indent="yes"/>


<xsl:template match="/">

  <xsl:if test="$platform/parameters/parameter[@name='generate-meta-inf-orm']='true'">

    <xsl:comment> Generated by PVEntityGenerator </xsl:comment>

    <xsl:choose>
      <xsl:when test="$platform/parameters/parameter[@name='jpa-version']='2.0'">

        <entity-mappings
           xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
           xsi:schemaLocation="http://java.sun.com/xml/ns/persistence/orm http://java.sun.com/xml/ns/persistence/orm_2_0.xsd" 
           version="2.0">
          
          <xsl:call-template name="generate-orm-content"/>
    
        </entity-mappings>

      </xsl:when>
      <xsl:otherwise>

        <entity-mappings
           xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
           xsi:schemaLocation="http://java.sun.com/xml/ns/persistence/orm http://java.sun.com/xml/ns/persistence/orm_1_0.xsd"
           version="1.0">
          
          <xsl:call-template name="generate-orm-content"/>
    
        </entity-mappings>
    
      </xsl:otherwise>
    </xsl:choose>

  </xsl:if>

</xsl:template>

<xsl:template name="generate-orm-content">
  <!-- table generator defintions -->
  <xsl:for-each select="$db-definition/entities/entity[keys/primary-key]">

    <xsl:variable name="generate-entity" select="key('generate-entity', @name)"/>
    <xsl:variable name="generate-entity-platform" select="key('generate-entity-platform', @name)"/>

    <xsl:if test="$generate-entity/parameters/parameter[@name='generate-entity']='true'">

      <xsl:variable name="counter-name">
        <xsl:choose>
          <xsl:when test="$platform/parameters/parameter[@name='database-counter-name-primarykey']='true'">
            <xsl:value-of select="keys/primary-key/attribute-ref/@attribute"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@name"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:choose>
        <xsl:when test="$identity-strategy='sequence'">

          <sequence-generator name="{@name}" sequence-name="{@name}_sq"/>

        </xsl:when>
        <xsl:otherwise>

          <table-generator name="{@name}" table="{$platform/parameters/parameter[@name='database-counter-table']}"
             pk-column-name="Counter" pk-column-value="{$counter-name}"
             value-column-name="LastValue" allocation-size="{$generate-entity-platform/parameters/parameter[@name='allocation-size']}"/>

        </xsl:otherwise>
      </xsl:choose>

    </xsl:if>

  </xsl:for-each>
</xsl:template>


</xsl:stylesheet>
