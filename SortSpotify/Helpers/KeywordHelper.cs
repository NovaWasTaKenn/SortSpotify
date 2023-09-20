using System.Text.RegularExpressions;

namespace SortSpotify.Helpers
{
    public class KeywordHelper
    {

        public bool FindKeywords(string text, string keywords)
        {
            Queue<string> keywordsQueue = new Queue<string>(Regex.Split(keywords, @"(\||&|\(|\))"));

            

            bool result = text.Contains(keywordsQueue.Dequeue());

            int count = keywordsQueue.Count;

            //if(count%2 != 0)
            //{
            //    throw new InvalidDataException("Invalid keyword string");
            //}
            //

            Stack<Tuple<bool, string>> results = new Stack<Tuple<bool, string>>();

            string currentToken = "";
            string _operator = ""; 

            while (keywordsQueue.TryPeek(out string nextValue))
            {
                string nextToken= keywordsQueue.Dequeue();

                if (nextToken == "(")
                {
                    results.Push(Tuple.Create(result, currentToken));
                    result = text.Contains(keywordsQueue.Dequeue());
                }

                currentToken = nextToken;

                if (currentToken == ")")
                {
                    (bool oldResult, string oldOperator) = results.Pop();
                    result = oldOperator == "|" ? (result | oldResult) : (result & oldResult);

                }

                if(currentToken == "|" | currentToken == "&") 
                {
                    _operator = currentToken;

                }

                if (currentToken != "|" & currentToken != "&" & currentToken != "(" & currentToken != ")")
                {

                    string word = currentToken;
                    result = _operator == "|" ? (result | text.Contains(word)) : (result & text.Contains(word));


                }


            }

            return result;

        }

    }
}
