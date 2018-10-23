package com.mycompany.app;

/**
 * App's summary
 */
public class App<T> {
    /**
     * Main's summary,
     * continued from the line above
     * <p>
     * It needs a `<p>` to start another line
     */
    public static void main(String[] args) {
        System.out.println("Hello World!");
    }

    /**
     * Test a list:
     * - first item
     * - second item
     */
    public void testCommentsWithList() {
    }

    /**
     * <p>Write the list with HTML tag in Summary:</p>
     * <ul>
     *     <li>first item</li>
     *     <li>second item</li>
     * </ul>
     */
    public void testCommentsWithHtmlTag() {
    }

    /**
     * The **apiNote** section should be extracted into **remarks** property
     * @apiNote
     * ## examples\n
     * Here is a sample code:\n
     * [!code-java[Sample_Code](~/_sample/APITests.java?name={Sample_code1} "Sample for ContainerURL.create")]
     */
    public void testCommentsWithApiNote() {
    }

    /**
     * This is first line.
     * <br>
     * This is second line.
     */
    public void testCommentsWithBr() {
    }

    /**
     * Test external link. See:
     * <a href="https://dotnet.github.io/docfx/">DocFX</a>
     */
    public void testCommentsWithExternalLink() {
    }

    /**
     * @apiNote
     * Use pre help keep the indentation in code snippet whithin apiNote
     * <pre>
     * ```Java\n
     * // No indentation for line 1 and 2\n
     * public void checkIndentation() {\n
     *     // 4 spaces indentation
     *         // 8 spaces indentation
     * }\n
     * ```\n
     * </pre>
     */
    public void testIndentationWithPre() {
    }

    /**
     * @apiNote
     * >[!NOTE]
     * Here is a Note with a list below:
     * - item 1
     * - item 2
     */
    public void testNOTEFormat() {
    }

    /**
     * Not decoded for summary: `<`, `>`
     * @apiNote
     * Decoded in remarks: `<`, `>`\n
     * ```Java\n
     * //Decoded in code snippet of remarks: `<`, `>`\n
     * ```\n
     */
    public void testEncode() {
    }

    /**
     * An Inner class to test file name format and refid/id format.
     * The fileName/refid/id should not contain `_` or hash or other that not exist in its origin name
     */
    public class testIfCode2YamlIsCorrectlyConvertFileNameAndIdToRegularizedCompoundNameForLongFileNamesThatWillBeConvertedToHashByDoxygen { 
    }
}
