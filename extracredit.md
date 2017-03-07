# Extra Credit
#### CSS
To make my search results page beautiful, I use [Material Components](https://github.com/material-components/material-components-web). Using Material Components allows me to give my page a feel similar to Google's Materialize by using text, objects, and colors that are based on Materialize. For NBA stats, I also show a photo of the player using an [NBA headshot api](https://github.com/hlyford/nba-headshot-api). If there is no photo found for the player, I show a default image instead.

#### Body Snippet & Query Words
To allow a user to quickly scan a description of a page, I bold any of their query search words in the description. When a user searches and the results are returned, I go through each description and check each word against the words in the query to see if they match. The description is taken from the meta tag in a page's header if one is provided. If `description` is not found, then I check `og:description`. If that is also not provided, I use the title of the article.

#### AJAX
The use of AJAX allows for a better user experience that is more similar to a Google search. When a user stops typing (even if they do not hit search or enter), the results page will be updated with their query. However, I make sure to only perform the search when the user has stopped typing for half a second. This prevents searching from happening on a term after the user is typing quickly and has already typed more characters on to the term. User searches are only saved for search suggestions when the user actually hits enter or the search button.