# data.literaturetime

Clock using time quotes from literature, inspired by [JohannesNE](https://github.com/JohannesNE/literature-clock) and [Jaap Meijers](http://www.eerlijkemedia.nl/) and [E-reader clock](https://www.instructables.com/id/Literary-Clock-Made-From-E-reader/). The quotes are from the list compiled by Jaap Meijers and further cleaned up.

The origin of the literature quotes are, and I quote from https://www.instructables.com/Literary-Clock-Made-From-E-reader/

> In 2011, newspaper The Guardian asked its readers to submit quotes from books which mention times. They wanted to build an installation for a literary festival. So they have two versions of a list on their website ([1](https://www.theguardian.com/books/table/2011/apr/21/literary-clock?CMP=twt_gu), [2](https://www.guardian.co.uk/books/booksblog/2011/apr/15/christian-marclay-the-clock-literature)).
>
> I combined the two lists, cleaned them up, added a few times I found myself, and turned them into one CSV file.
>
> Unfortunately the list does not cover all minutes of the day. I worked around this by using some quotes more than once, for instance if it can be used both in the AM and PM. More vague time indications can be used around a certain time, so this quote from Catcher in the Rye is used at 9.58AM: "I didn't sleep too long, because I think it was only around ten o'clock when I woke up ... "

This is the backend for [literature.artsoftheinsane.com](https://literature.artsoftheinsane.com) which feeds redis with the literature quotes

The kubernetes definition files can be found in the repo [kubernetes.literaturetime](https://github.com/blomma/kubernetes.literaturetime)

All of this running on a [k3s.io](https://k3s.io) kubernetes cluster on top of a couple of raspberry pi's.
