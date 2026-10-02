const express = require("express");

const router = express.Router();

const userController = require("../controllers/users");
const authMiddleware = require("../middleware/auth");

router.get(
  "/",
  authMiddleware,
  userController.getUser
);

router.get(
  "/all",
  authMiddleware,
  userController.getUsers
);

router.put(
  "/profile",
  authMiddleware,
  userController.updateProfile
);

module.exports = router;