<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  version="1.0"
>

<xsl:output method="html" encoding="UTF-8"/>


<!-- ### root html ### -->
<xsl:template match="pventitygenerator-data">
<html>
  <xsl:call-template name="html_head"/>
  <body>

    <h1><xsl:value-of select="project-settings/parameters/parameter[@name='project-name']"/></h1>
    <xsl:if test="project-settings/parameters/parameter[@name='version']!=''">
      <xsl:text>Version </xsl:text><xsl:value-of select="project-settings/parameters/parameter[@name='version']"/>
    </xsl:if>

    <p>
    <xsl:for-each select="db-definition/entities/entity">
      <xsl:if test="position() &gt; 1"> | </xsl:if>
      <a href="#{@name}"><xsl:value-of select="@name"></xsl:value-of></a>
    </xsl:for-each>
    </p>

    <xsl:apply-templates select="db-definition/entities/entity"/>

  </body>
</html>
</xsl:template>

<!-- html head -->
<xsl:template name="html_head">
  <head>
    <title><xsl:value-of select="project-settings/parameters/parameter[@name='project-name']"/></title>
    <style>
      BODY { COLOR: #000000; FONT-FAMILY: Verdana, Helvetica; FONT-SIZE: 11px }
      P { COLOR: #000000; FONT-FAMILY: Verdana, Helvetica; FONT-SIZE: 11px }
      TD { COLOR: #000000; FONT-FAMILY: Verdana, Helvetica; FONT-SIZE: 11px }
      TH { COLOR: #ffffff; FONT-FAMILY: Verdana, Helvetica; FONT-SIZE: 11px; TEXT-ALIGN: left; }
      H1 { FONT-SIZE: 24px }
      H2 { FONT-SIZE: 20px }
      H3 { FONT-SIZE: 15px }
      .row_header { background-color:#0000aa }
      .row_even { background-color:#f3f3f3; vertical-align:top; }
      .row_odd { background-color:#ffffff; vertical-align:top; }
    </style>
  </head>
</xsl:template>

<!-- entity -->
<xsl:template match="entity">
  <xsl:variable name="entity" select="."/>
  <xsl:variable name="generate-entity" select="/pventitygenerator-data/project-settings/entity-generation/generate-entity
      [@entity=$entity/@name]"/>

  <hr/>

  <a name="{@name}"><h2><xsl:value-of select="@name"/></h2></a>
  <xsl:if test="@description">
    <p><xsl:value-of select="@description"/></p>
  </xsl:if>

  <h3>Attributes</h3>
  <table border="0" cellpadding="3" cellspacing="0">
    <tr class="row_header">
      <th>Name</th>
      <th>Type</th>
      <th>Required</th>
      <th>Default</th>
      <th>Description</th>
    </tr>
    <xsl:for-each select="attributes/attribute">
      <tr>
        <xsl:attribute name="class">
          <xsl:choose>
            <xsl:when  test="position() mod 2 = 0">row_even</xsl:when>
            <xsl:otherwise>row_odd</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <td><xsl:value-of select="@name"/></td>
        <td nowrap="nowrap">
          <xsl:value-of select="@type"/>
          <xsl:if test="@size">
            <xsl:text>[</xsl:text>
            <xsl:value-of select="@size"/>
            <xsl:if test="@scale"><xsl:text>/</xsl:text><xsl:value-of select="@scale"/></xsl:if>
            <xsl:text>]</xsl:text>
          </xsl:if>
        </td>
        <td align="center"><xsl:if test="@required='true'">X</xsl:if></td>
        <td><xsl:value-of select="@default"/></td>
        <td>
          <xsl:choose>
            <xsl:when test="$entity/keys/primary-key/attribute-ref[@attribute=current()/@name]">
              <em>Primary Key </em>
            </xsl:when>
            <xsl:when test="@name='vstamp'">
              <em>Version timestamp </em>
            </xsl:when>
          </xsl:choose>
          <xsl:value-of select="@description"/>
        </td>
      </tr>
    </xsl:for-each>
  </table>

  <xsl:if test="keys/foreign-key">
    <h3>Foreign Keys</h3>
    <table border="0" cellpadding="3" cellspacing="0">
      <tr class="row_header">
        <th>Left side</th>
        <th>Type</th>
        <th>Right side</th>
        <th>Cascading delete</th>
      </tr>
      <xsl:for-each select="keys/foreign-key">
        <tr>
          <xsl:attribute name="class">
            <xsl:choose>
              <xsl:when  test="position() mod 2 = 0">row_even</xsl:when>
              <xsl:otherwise>row_odd</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <td>
            <xsl:value-of select="@foreign-entity"/>
            <xsl:text> (</xsl:text>
            <xsl:for-each select="attribute-ref">
              <xsl:if test="position() &gt; 1">, </xsl:if>
              <xsl:value-of select="@foreign-attribute"/>
            </xsl:for-each>
            <xsl:text>)</xsl:text>
          </td>
          <td style="width:50px;" nowrap="nowrap">
            <xsl:choose>
              <xsl:when test="$entity/keys/unique-key[attribute-ref/@attribute=current()/attribute-ref/@attribute]">1 : 1</xsl:when>
              <xsl:otherwise>1 : &#x221E;</xsl:otherwise>
            </xsl:choose>
          </td>
          <td>
            <xsl:value-of select="$entity/@name"/>
            <xsl:text> (</xsl:text>
            <xsl:for-each select="attribute-ref">
              <xsl:if test="position() &gt; 1">, </xsl:if>
              <xsl:value-of select="@attribute"/>
            </xsl:for-each>
            <xsl:text>)</xsl:text>
          </td>
          <td align="center"><xsl:if test="@cascading-delete='true'">X</xsl:if></td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:if>

  <xsl:if test="indexes/index">
    <h3>Indexes</h3>
    <table border="0" cellpadding="3" cellspacing="0">
      <tr class="row_header">
        <th>Column</th>
        <th>Unique</th>
      </tr>
      <xsl:for-each select="indexes/index">
        <tr>
          <xsl:attribute name="class">
            <xsl:choose>
              <xsl:when  test="position() mod 2 = 0">row_even</xsl:when>
              <xsl:otherwise>row_odd</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <td>
            <xsl:for-each select="attribute-ref">
              <xsl:if test="position() &gt; 1">, </xsl:if>
              <xsl:value-of select="@attribute"/>
            </xsl:for-each>
          </td>
          <td align="center"><xsl:if test="@unique='true'">X</xsl:if></td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:if>

  <xsl:if test="$generate-entity/enumeration-entries/enumeration-entry">
    <h3>Enumeration entries</h3>
    <table border="0" cellpadding="3" cellspacing="0">
      <tr class="row_header">
        <th>Value</th>
        <th>Identifier</th>
        <th>Name</th>
        <th>Description</th>
      </tr>
      <xsl:for-each select="$generate-entity/enumeration-entries/enumeration-entry">
        <tr>
          <xsl:attribute name="class">
            <xsl:choose>
              <xsl:when  test="position() mod 2 = 0">row_even</xsl:when>
              <xsl:otherwise>row_odd</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <td><xsl:value-of select="@id"/></td>
          <td><xsl:value-of select="@identifier"/></td>
          <td><xsl:value-of select="@name"/></td>
          <td><xsl:value-of select="@description"/></td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:if>

  <br/>

</xsl:template>

</xsl:stylesheet>
