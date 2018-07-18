package com.mycompany.app;

/**
 * App's summary
 */
public class App {
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
     * <pre>
     * Use pre help keep the indentation in code snippet whithin apiNote
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
}
