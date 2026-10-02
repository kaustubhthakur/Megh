const User = require("../models/User");

exports.getUser = async (req, res) => {
  try {
    const user = await User.getUser(req.user.id);

    if (!user) {
      return res.status(404).json({
        message: "User not found",
      });
    }

    return res.status(200).json({
      user: {
        id: user.id,
        username: user.username,
        email: user.email,
        phone: user.phone,
        avatar: user.avatar,
        is_vendor: user.is_vendor,
        email_verified: user.email_verified,
        phone_verified: user.phone_verified,
        onboarding_completed: user.onboarding_completed,
      },
    });
  } catch (error) {
    console.error("Get user error:", error);

    return res.status(500).json({
      message: "Something went wrong",
    });
  }
};

exports.updateProfile = async (req, res) => {
  try {
    const {
      username,
      avatar,
      phone,
    } = req.body;

    const user = await User.updateUser(
      req.user.id,
      {
        username,
        avatar,
        phone,
      }
    );

    if (!user) {
      return res.status(404).json({
        message: "User not found",
      });
    }

    return res.status(200).json({
      message: "Profile updated successfully",
      user: {
        id: user.id,
        username: user.username,
        email: user.email,
        phone: user.phone,
        avatar: user.avatar,
        is_vendor: user.is_vendor,
        email_verified: user.email_verified,
        phone_verified: user.phone_verified,
        onboarding_completed: user.onboarding_completed,
      },
    });
  } catch (error) {
    console.error("Update profile error:", error);

    return res.status(500).json({
      message: "Something went wrong",
    });
  }
};