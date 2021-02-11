const express = require('express');
const app = express();

app.get('/', (req, res) =>
{
    res.redirect("https://teambattles.fortnite.com/leaderboards");
});

app.get('/ping', (req, res) => {
    res.send("pong");
});

app.listen(8080, r => {
    console.log("Server listening on port 8080");
});