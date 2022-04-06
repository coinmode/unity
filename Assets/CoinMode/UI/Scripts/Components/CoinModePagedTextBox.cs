using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM PagedTextWindow")]
    public class CoinModePagedTextBox : CoinModeUIBehaviour
    {
        [SerializeField]
        private ScrollRect scrollRect = null;

        [SerializeField]
        private CoinModeText targetText = null;

        [SerializeField]
        private CoinModeButton nextPageButton = null;

        [SerializeField]
        private CoinModeButton previousPageButton = null;

        [SerializeField]
        private CoinModeText pageNumbersText = null;

        [SerializeField]
        private string appendedNextPageString = "... Continues on following page.";

        [SerializeField]
        private int characterLimit = 5000;

        public int currentPage { get; private set; } = 0;

        private string[] words = null;
        private List<string> pagedText = new List<string>();
        private StringBuilder pageStringBuilder;

        protected override void Awake()
        {
            base.Awake();
            nextPageButton.onClick.AddListener(NextPage);
            previousPageButton.onClick.AddListener(PreviousPage);
        }

        public void SetText(string sourceText)
        {
            pagedText.Clear();
            currentPage = 0;
            words = sourceText.Split(' ');
            try
            {
                pageStringBuilder = new StringBuilder(characterLimit + appendedNextPageString.Length + Environment.NewLine.Length);
                for (int i = 0; i < words.Length; i++)
                {
                    if (pageStringBuilder.Length + words[i].Length < characterLimit)
                    {
                        if (pageStringBuilder.Length == 0)
                        {
                            pageStringBuilder.Append(words[i]);
                        }
                        else
                        {
                            pageStringBuilder.Append(' ');
                            pageStringBuilder.Append(words[i]);
                        }
                    }
                    else
                    {
                        pageStringBuilder.Append(Environment.NewLine);
                        pageStringBuilder.Append(appendedNextPageString);
                        pagedText.Add(pageStringBuilder.ToString());
                        pageStringBuilder.Clear();
                        pageStringBuilder.Append(words[i]);
                    }
                }

                if(pageStringBuilder.Length > 0 )
                {
                    pagedText.Add(pageStringBuilder.ToString());
                    pageStringBuilder.Clear();
                }

                targetText.text = pagedText[0];
                scrollRect.verticalNormalizedPosition = 1.0F;
                SetUpUI();
            }    
            catch
            {
                CoinModeLogging.LogWarning("CoinModePagedTextWindow", "SetText", "Unable to set text, exception thrown building string, individual words must not be longer than the character limit!");
            }
        }

        public void NavigateToPage(int pageIndex)
        {
            if (pagedText.Count > 1)
            {
                if(pageIndex >= 0 && pageIndex < pagedText.Count)
                {
                    scrollRect.verticalNormalizedPosition = 1.0F;
                    currentPage = pageIndex;
                    targetText.text = pagedText[currentPage];
                    pageNumbersText.text = GetPageNumbersString();
                    if(currentPage < pagedText.Count - 1 && currentPage >= 1)
                    {
                        previousPageButton.interactable = true;
                        nextPageButton.interactable = true;
                    }
                    else if(currentPage == 0)
                    {
                        previousPageButton.interactable = false;
                        nextPageButton.interactable = true;
                    }
                    else
                    {
                        previousPageButton.interactable = true;
                        nextPageButton.interactable = false;
                    }
                }
            }
        }

        public void NextPage()
        {
            NavigateToPage(currentPage + 1);
        }

        public void PreviousPage()
        {
            NavigateToPage(currentPage - 1);
        }

        private void SetUpUI()
        {
            if (pagedText.Count > 1)
            {
                previousPageButton.gameObject.SetActive(true);
                previousPageButton.interactable = false;
                nextPageButton.gameObject.SetActive(true);
                nextPageButton.interactable = true;
                pageNumbersText.gameObject.SetActive(true);
                pageNumbersText.text = GetPageNumbersString();                
            }
            else
            {
                previousPageButton.gameObject.SetActive(false);
                nextPageButton.gameObject.SetActive(false);
                pageNumbersText.gameObject.SetActive(false);
            }
        }

        private string GetPageNumbersString()
        {
            return string.Format("Page {0} of {1}", currentPage + 1, pagedText.Count);
        }
    }
}