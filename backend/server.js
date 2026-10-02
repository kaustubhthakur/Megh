require("dotenv").config({ override: true });
const express = require("express");
const cors = require("cors");
const cookieParser = require("cookie-parser");
const http = require("http");

const authRouter = require("./routes/auth");
const userRouter = require('./routes/user')
const path = require("path");
const PORT = process.env.PORT || 8081;
console.log("CWD:", process.cwd());
console.log("__dirname:", __dirname);
const app = express();

app.use(
  cors({
    origin:
      process.env.FRONTEND_URL ||
      "http://localhost:3000",
    credentials: true,
  })
);

app.use(express.json());
app.use(cookieParser());

app.use("/auth", authRouter);
app.use("/user",userRouter)

const server = http.createServer(app);

server.listen(PORT, () => {
  console.log(`Server running on port ${PORT}...`);
});