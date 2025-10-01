using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "CustomTMPValidator", menuName = "Input Field Validator/Custom Validator")]
public class CustomTMPValidator : TMP_InputValidator
{
    public override char Validate(ref string text, ref int pos, char ch)
    {
        // Example: Allow only alphanumeric characters
        if (char.IsLetterOrDigit(ch) || "�������������������������������������Ũ��������������������������".Contains(ch))
        {
            text = text.Insert(pos, ch.ToString());
            pos++;
            return ch;
        }
        return (char)0; // Reject the character
    }
}