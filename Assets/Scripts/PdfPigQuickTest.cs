using UnityEngine;
using UglyToad.PdfPig;
using System.IO;

public class PdfPigQuickTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("PdfPig version check: Library is present!");

        Debug.Log("PdfPig namespace is accessible ✓");

        string testPdfPath = Path.Combine(Application.streamingAssetsPath, "test.pdf");

        if (File.Exists(testPdfPath))
        {
            try
            {
                using (PdfDocument document = PdfDocument.Open(testPdfPath))
                {
                    Debug.Log($"Successfully opened PDF! Number of pages: {document.NumberOfPages}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to open test PDF: " + ex.Message);
            }
        }
        else
        {
            Debug.LogWarning("No test.pdf found in StreamingAssets. " +
                             "Place a small PDF there to fully test opening.");
        }
    }
}
