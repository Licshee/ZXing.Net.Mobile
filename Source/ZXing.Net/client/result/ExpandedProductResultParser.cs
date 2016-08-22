/*
 * Copyright (C) 2010 ZXing authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/*
 * These authors would like to acknowledge the Spanish Ministry of Industry,
 * Tourism and Trade, for the support in the project TSI020301-2008-2
 * "PIRAmIDE: Personalizable Interactions with Resources on AmI-enabled
 * Mobile Dynamic Environments", led by Treelogic
 * ( http://www.treelogic.com/ ):
 *
 *   http://www.piramidepse.com/
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace ZXing.Client.Result
{
    /// <summary>
    /// Parses strings of digits that represent a RSS Extended code.
    /// </summary>
    /// <author>Antonio Manuel Benjumea Conde, Servinform, S.A.</author>
    /// <author>Agustín Delgado, Servinform, S.A.</author>
    public class ExpandedProductResultParser : ResultParser
    {
        override public ParsedResult parse(ZXing.Result result)
        {
            BarcodeFormat format = result.BarcodeFormat;
            if (format != BarcodeFormat.RSS_EXPANDED)
            {
                // ExtendedProductParsedResult NOT created. Not a RSS Expanded barcode
                return null;
            }
            var rawText = result.Text;

            string productID = null;
            string sscc = null;
            string lotNumber = null;
            string productionDate = null;
            string packagingDate = null;
            string bestBeforeDate = null;
            string expirationDate = null;
            string weight = null;
            string weightType = null;
            string weightIncrement = null;
            string price = null;
            string priceIncrement = null;
            string priceCurrency = null;
            var uncommonAIs = new Dictionary<string, string>();

            int i = 0;

            while (i < rawText.Length)
            {
                var ai = findAIvalue(i, rawText);
                if (ai == null)
                {
                    // Error. Code doesn't match with RSS expanded pattern
                    // ExtendedProductParsedResult NOT created. Not match with RSS Expanded pattern
                    return null;
                }
                i += ai.Length + 2;
                var value = findValue(i, rawText);
                i += value.Length;

                switch (ai)
                {
                    case "00":
                        sscc = value;
                        break;
                    case "01":
                        productID = value;
                        break;
                    case "10":
                        lotNumber = value;
                        break;
                    case "11":
                        productionDate = value;
                        break;
                    case "13":
                        packagingDate = value;
                        break;
                    case "15":
                        bestBeforeDate = value;
                        break;
                    case "17":
                        expirationDate = value;
                        break;
                    case "3100":
                    case "3101":
                    case "3102":
                    case "3103":
                    case "3104":
                    case "3105":
                    case "3106":
                    case "3107":
                    case "3108":
                    case "3109":
                        weight = value;
                        weightType = ExpandedProductParsedResult.KILOGRAM;
                        weightIncrement = ai.Substring(3);
                        break;
                    case "3200":
                    case "3201":
                    case "3202":
                    case "3203":
                    case "3204":
                    case "3205":
                    case "3206":
                    case "3207":
                    case "3208":
                    case "3209":
                        weight = value;
                        weightType = ExpandedProductParsedResult.POUND;
                        weightIncrement = ai.Substring(3);
                        break;
                    case "3920":
                    case "3921":
                    case "3922":
                    case "3923":
                        price = value;
                        priceIncrement = ai.Substring(3);
                        break;
                    case "3930":
                    case "3931":
                    case "3932":
                    case "3933":
                        if (value.Length < 4)
                        {
                            // The value must have more of 3 symbols (3 for currency and
                            // 1 at least for the price)
                            // ExtendedProductParsedResult NOT created. Not match with RSS Expanded pattern
                            return null;
                        }
                        price = value.Substring(3);
                        priceCurrency = value.Substring(0, 3);
                        priceIncrement = ai.Substring(3);
                        break;
                    default:
                        uncommonAIs[ai] = value;
                        break;
                }
            }

            return new ExpandedProductParsedResult(rawText,
                                                   productID,
                                                   sscc,
                                                   lotNumber,
                                                   productionDate,
                                                   packagingDate,
                                                   bestBeforeDate,
                                                   expirationDate,
                                                   weight,
                                                   weightType,
                                                   weightIncrement,
                                                   price,
                                                   priceIncrement,
                                                   priceCurrency,
                                                   uncommonAIs);
        }

        private static string findAIvalue(int i, string rawText)
        {
            var c = rawText[i];
            // First character must be a open parenthesis.If not, ERROR
            if (c != '(')
            {
                return null;
            }

            var rawTextAux = rawText.Substring(i + 1);
            var buf = new StringBuilder();

            for (int index = 0; index < rawTextAux.Length; index++)
            {
                var currentChar = rawTextAux[index];
                if (currentChar == ')')
                {
                    return buf.ToString();
                }
                else if (currentChar >= '0' && currentChar <= '9')
                {
                    buf.Append(currentChar);
                }
                else
                {
                    return null;
                }
            }
            return buf.ToString();
        }

        private static string findValue(int i, string rawText)
        {
            var buf = new StringBuilder();
            var rawTextAux = rawText.Substring(i);

            for (int index = 0; index < rawTextAux.Length; index++)
            {
                var c = rawTextAux[index];
                if (c == '(')
                {
                    // We look for a new AI. If it doesn't exist (ERROR), we coninue
                    // with the iteration
                    if (findAIvalue(index, rawTextAux) == null)
                    {
                        buf.Append('(');
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    buf.Append(c);
                }
            }
            return buf.ToString();
        }
    }
}