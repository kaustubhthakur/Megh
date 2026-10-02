const User = require("../models/User");

module.exports = async (req, res, next) => {
  try {
    const user = await User.findUserById(req.user.id);

    if (!user) {
      return res.status(404).json({
        message: "User not found",
      });
    }

    if (user.is_vendor) {
      return res.status(403).json({
        message: "Normal user access required",
      });
    }

    req.user = user;

    next();
  } catch (error) {
    console.error("User middleware error:", error);

    return res.status(500).json({
      message: "Something went wrong",
    });
  }
};