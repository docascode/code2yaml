<?xml version="1.0"?>
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  exclude-result-prefixes="xsl"
  version="1.1">

  <xsl:output method="xml" indent="yes" encoding="UTF-8" />

  <xsl:template match="itemizedlist">
    <ul>
      <xsl:apply-templates />
    </ul>
  </xsl:template>

  <xsl:template match="listitem">
    <li>
      <xsl:apply-templates />
    </li>
  </xsl:template>

  <xsl:template match="bold">
    <b>
      <xsl:apply-templates />
    </b>
  </xsl:template>

  <xsl:template match="emphasis">
    <em>
      <xsl:apply-templates />
    </em>
  </xsl:template>

  <xsl:template match="preformatted">
      <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="subscript">
    <sub>
      <xsl:apply-templates />
    </sub>
  </xsl:template>

  <xsl:template match="superscript">
    <sup>
      <xsl:apply-templates />
    </sup>
  </xsl:template>

  <xsl:template match="programlisting">
    <code>
      <xsl:apply-templates />
    </code>
  </xsl:template>

  <xsl:template match="codeline">
    <xsl:apply-templates />
    <xsl:text>
    </xsl:text>
  </xsl:template>

  <xsl:template match="highlight">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="sp">
    <xsl:text>&#160;</xsl:text>
  </xsl:template>

  <xsl:template match="para">
    <p>
      <xsl:apply-templates />
    </p>
  </xsl:template>

  <xsl:template match="b">
    <strong>
      <xsl:apply-templates />
    </strong>
  </xsl:template>

  <xsl:template match="i">
    <em>
      <xsl:apply-templates />
    </em>
  </xsl:template>

  <xsl:template match="ui">
    <strong>
      <xsl:apply-templates />
    </strong>
  </xsl:template>

  <xsl:template match="c">
    <code>
      <xsl:apply-templates />
    </code>
  </xsl:template>

  <xsl:template match="computeroutput">
    <code>
      <xsl:apply-templates />
    </code>
  </xsl:template>

  <xsl:template match="code">
    <pre>
      <code>
        <xsl:if test="normalize-space(@language)">
          <xsl:attribute name="class">
            <xsl:value-of select="@language" />
          </xsl:attribute>
        </xsl:if>
        <xsl:apply-templates />
      </code>
    </pre>
  </xsl:template>

  <xsl:template match="value">
    <returns>
      <xsl:apply-templates />
    </returns>
  </xsl:template>

  <xsl:template match="ulink">
    <a>
      <xsl:attribute name="href">
        <xsl:value-of select="@url"/>
      </xsl:attribute>
      <xsl:value-of select="current()"/>
    </a>
  </xsl:template>

  <xsl:template match="ref">
    <xref>
      <xsl:attribute name="uid">
        <xsl:value-of select="@refid"/>
      </xsl:attribute>
      <xsl:attribute name="data-throw-if-not-resolved">
        <xsl:text>false</xsl:text>
      </xsl:attribute>
      <xsl:value-of select="current()"/>
    </xref>
  </xsl:template>

  <xsl:template match="paramref">
    <xsl:if test="normalize-space(@name)">
      <em>
        <xsl:value-of select="@name" />
      </em>
    </xsl:if>
  </xsl:template>

  <xsl:template match="typeparamref">
    <xsl:if test="normalize-space(@name)">
      <em>
        <xsl:value-of select="@name" />
      </em>
    </xsl:if>
  </xsl:template>

  <xsl:template match="list">
    <xsl:variable name="listtype">
      <xsl:value-of select="normalize-space(@type)"/>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$listtype = 'table'">
        <table>
          <xsl:if test="listheader">
            <thead>
              <tr>
                <th>
                  <xsl:apply-templates select="listheader/term" />
                </th>
                <th>
                  <xsl:apply-templates select="listheader/description" />
                </th>
              </tr>
            </thead>
          </xsl:if>
          <tbody>
            <xsl:for-each select="item">
              <tr>
                <td>
                  <xsl:apply-templates select="term"/>
                </td>
                <td>
                  <xsl:apply-templates select="description"/>
                </td>
              </tr>
            </xsl:for-each>
          </tbody>
        </table>
      </xsl:when>
      <xsl:otherwise>
        <xsl:if test="listheader">
          <p>
            <strong>
              <xsl:if test="listheader/term">
                <xsl:value-of select="concat(string(listheader/term),'-')"/>
              </xsl:if>
              <xsl:value-of select="string(listheader/description)" />
            </strong>
          </p>
        </xsl:if>
        <xsl:choose>
          <xsl:when test="$listtype = 'bullet'">
            <ul>
              <xsl:for-each select="item">
                <li>
                  <xsl:apply-templates select="term" />
                  <xsl:apply-templates select="description" />
                </li>
              </xsl:for-each>
            </ul>
          </xsl:when>
          <xsl:when test="$listtype = 'number'">
            <ol>
              <xsl:for-each select="item">
                <li>
                  <xsl:apply-templates select="term" />
                  <xsl:apply-templates select="description" />
                </li>
              </xsl:for-each>
            </ol>
          </xsl:when>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="description">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="term">
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="linebreak">
  </xsl:template>

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>