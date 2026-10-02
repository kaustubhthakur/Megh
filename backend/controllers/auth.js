const bcrypt = require("bcryptjs");
const jwt = require("jsonwebtoken");

const User = require("../models/User");

const JWT_SECRET = process.env.JWT_SECRET;

exports.register = async (req, res) => {
  try {
    const {
      username,
      email,
      password,
      phone,
      isVendor = false,
    } = req.body;

    if (!username || !email || !password) {
      return res.status(400).json({
        message: "Username, email and password are required",
      });
    }

    const existingEmail = await User.findUserByEmail(email);

    if (existingEmail) {
      return res.status(409).json({
        message: "Email already registered",
      });
    }

    const existingUsername =
      await User.findUserByUsername(username);

    if (existingUsername) {
      return res.status(409).json({
        message: "Username already taken",
      });
    }

    if (phone) {
      const existingPhone =
        await User.findUserByPhone(phone);

      if (existingPhone) {
        return res.status(409).json({
          message: "Phone number already registered",
        });
      }
    }

    const hashedPassword = await bcrypt.hash(
      password,
      12
    );

    const user = await User.createUser({
      username,
      email,
      password: hashedPassword,
      phone,
      isVendor,
    });

    const token = jwt.sign(
      {
        userId: user.id,
      },
      JWT_SECRET,
      {
        expiresIn: "7d",
      }
    );

    return res.status(201).json({
      message: "Registration successful",
      token,
      user: {
        id: user.id,
        username: user.username,
        email: user.email,
        phone: user.phone,
        is_vendor: user.is_vendor,
        email_verified: user.email_verified,
        phone_verified: user.phone_verified,
        onboarding_completed:
          user.onboarding_completed,
      },
    });
  } catch (error) {
    console.error("Register error:", error);

    return res.status(500).json({
      message: "Something went wrong",
    });
  }
};


exports.login = async (req, res) => {
  try {
    const {
      email,
      password,
    } = req.body;

    if (!email || !password) {
      return res.status(400).json({
        message: "Email and password are required",
      });
    }

    const user = await User.findUserByEmail(email);

    if (!user) {
      return res.status(401).json({
        message: "Invalid email or password",
      });
    }

    const passwordMatch = await bcrypt.compare(
      password,
      user.password
    );

    if (!passwordMatch) {
      return res.status(401).json({
        message: "Invalid email or password",
      });
    }

    const token = jwt.sign(
      {
        userId: user.id,
      },
      JWT_SECRET,
      {
        expiresIn: "7d",
      }
    );

    return res.status(200).json({
      message: "Login successful",
      token,
    });
  } catch (error) {
    console.error("Login error:", error);

    return res.status(500).json({
      message: "Something went wrong",
    });
  }
};


exports.sendOtp = async (req, res) => {
  try {
    const {
      email,
      method = "email",
    } = req.body;

    if (!email) {
      return res.status(400).json({
        message: "Email is required",
      });
    }

    if (!["email", "phone"].includes(method)) {
      return res.status(400).json({
        message: "Invalid OTP method",
      });
    }

    const user = await User.findUserByEmail(email);

    if (!user) {
      return res.status(404).json({
        message: "User not found",
      });
    }

    if (
      method === "email" &&
      user.email_verified
    ) {
      return res.status(400).json({
        message: "Email already verified",
      });
    }

    if (
      method === "phone" &&
      user.phone_verified
    ) {
      return res.status(400).json({
        message: "Phone already verified",
      });
    }

    const canResend = await User.canResendOtp(
      user.id,
      method
    );

    if (!canResend) {
      const resendTime =
        await User.getResendTime(
          user.id,
          method
        );

      return res.status(429).json({
        message: "Please wait before requesting another OTP",
        resend_in: resendTime,
      });
    }

    const otp = Math.floor(
      100000 + Math.random() * 900000
    ).toString();

    const expiresAt = new Date(
      Date.now() + 5 * 60 * 1000
    );

    await User.saveOtp(
      user.id,
      otp,
      expiresAt,
      method
    );

    console.log(
      `${method} OTP for ${email}: ${otp}`
    );

    return res.status(200).json({
      message: "OTP sent successfully",
      expires_in: 300,
    });
  } catch (error) {
    console.error("Send OTP error:", error);

    return res.status(500).json({
      message: "Something went wrong",
    });
  }
};


exports.logout = async (req, res) => {
  try {
    res.clearCookie("token");

    return res.status(200).json({
      message: "Logout successful",
    });
  } catch (error) {
    console.error("Logout error:", error);

    return res.status(500).json({
      message: "Something went wrong",
    });
  }
};