const jwt = require("jsonwebtoken");

const User = require("../models/User");

const JWT_SECRET = process.env.JWT_SECRET;

exports.auth = async (req, res, next) => {
  try {
    const authHeader = req.headers.authorization;

    if (!authHeader) {
      return res.status(401).json({
        message: "Authentication required",
      });
    }

    const token = authHeader.startsWith("Bearer ")
      ? authHeader.split(" ")[1]
      : authHeader;

    if (!token) {
      return res.status(401).json({
        message: "Authentication required",
      });
    }

    const decoded = jwt.verify(
      token,
      JWT_SECRET
    );

    const user = await User.findUserById(
      decoded.userId
    );

    if (!user) {
      return res.status(401).json({
        message: "User not found",
      });
    }

    req.user = user;

    next();
  } catch (error) {
    return res.status(401).json({
      message: "Invalid or expired token",
    });
  }
};

exports.isUser = (req, res, next) => {
  if (req.user.is_vendor) {
    return res.status(403).json({
      message: "Normal user access required",
    });
  }

  next();
};

exports.isVendor = (req, res, next) => {
  if (!req.user.is_vendor) {
    return res.status(403).json({
      message: "Vendor access required",
    });
  }

  next();
};