using System;

public static class HtmlBuilderUtil
{
    public static string Link(string url)
    {
        return
        $@"
        <!DOCTYPE html>
            <html lang='pl'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Redirektor</title>
            </head>
            <body>
                <span>Dzieki za klikniecie, zaraz zostaniesz przekierowany na: </span> <span id='link'></span>
            </body>
            <script>
            const link = 'https://{url}.pl';
            const linkElement = document.getElementById('link');
            linkElement.innerHTML = link;
            setTimeout(()=>" + "{window.location.replace(link)},1500)</script></html>";


    }
}