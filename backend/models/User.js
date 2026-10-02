const pool = require("../pool");

const USER_FIELDS = `
  id,
  username,
  email,
  phone,
  avatar,
  is_vendor,
  email_verified,
  phone_verified,
  onboarding_completed,
  created_at,
  updated_at
`;

// Create user
exports.createUser = async ({
  username,
  email,
  password,
  phone,
  isVendor = false,
}) => {
  const result = await pool.query(
    `
    INSERT INTO users (
      username,
      email,
      password,
      phone,
      avatar,
      is_vendor,
      email_verified,
      phone_verified,
      onboarding_completed
    )
    VALUES (
      $1,
      $2,
      $3,
      $4,
      NULL,
      $5,
      FALSE,
      FALSE,
      FALSE
    )
    RETURNING ${USER_FIELDS}
    `,
    [
      username,
      email,
      password,
      phone,
      isVendor,
    ]
  );

  return result.rows[0];
};


// Find user by email
exports.findUserByEmail = async (email) => {
  const result = await pool.query(
    `
    SELECT *
    FROM users
    WHERE email = $1
    `,
    [email]
  );

  return result.rows[0];
};


// Find user by phone
exports.findUserByPhone = async (phone) => {
  const result = await pool.query(
    `
    SELECT *
    FROM users
    WHERE phone = $1
    `,
    [phone]
  );

  return result.rows[0];
};


// Find user by username
exports.findUserByUsername = async (username) => {
  const result = await pool.query(
    `
    SELECT *
    FROM users
    WHERE username = $1
    `,
    [username]
  );

  return result.rows[0];
};


// Find user by ID
exports.findUserById = async (id) => {
  const result = await pool.query(
    `
    SELECT *
    FROM users
    WHERE id = $1
    `,
    [id]
  );

  return result.rows[0];
};


// Get user
exports.getUser = async (id) => {
  const result = await pool.query(
    `
    SELECT
      ${USER_FIELDS}
    FROM users
    WHERE id = $1
    `,
    [id]
  );

  return result.rows[0];
};


// Get all users
exports.getAllUsers = async () => {
  const result = await pool.query(
    `
    SELECT
      ${USER_FIELDS}
    FROM users
    ORDER BY created_at DESC
    `
  );

  return result.rows;
};


// Update user
exports.updateUser = async (
  id,
  {
    username,
    avatar,
    phone,
  }
) => {
  const result = await pool.query(
    `
    UPDATE users
    SET
      username = COALESCE($1, username),
      avatar = COALESCE($2, avatar),
      phone = COALESCE($3, phone),
      updated_at = NOW()
    WHERE id = $4
    RETURNING ${USER_FIELDS}
    `,
    [
      username,
      avatar,
      phone,
      id,
    ]
  );

  return result.rows[0];
};


// Verify email
exports.verifyEmail = async (userId) => {
  const result = await pool.query(
    `
    UPDATE users
    SET
      email_verified = TRUE,
      updated_at = NOW()
    WHERE id = $1
    RETURNING ${USER_FIELDS}
    `,
    [userId]
  );

  return result.rows[0];
};


// Verify phone
exports.verifyPhone = async (userId) => {
  const result = await pool.query(
    `
    UPDATE users
    SET
      phone_verified = TRUE,
      updated_at = NOW()
    WHERE id = $1
    RETURNING ${USER_FIELDS}
    `,
    [userId]
  );

  return result.rows[0];
};


// Mark vendor onboarding as completed
exports.completeOnboarding = async (userId) => {
  const result = await pool.query(
    `
    UPDATE users
    SET
      onboarding_completed = TRUE,
      updated_at = NOW()
    WHERE id = $1
      AND is_vendor = TRUE
    RETURNING ${USER_FIELDS}
    `,
    [userId]
  );

  return result.rows[0];
};


// Make user a vendor
exports.makeVendor = async (userId) => {
  const result = await pool.query(
    `
    UPDATE users
    SET
      is_vendor = TRUE,
      onboarding_completed = FALSE,
      updated_at = NOW()
    WHERE id = $1
    RETURNING ${USER_FIELDS}
    `,
    [userId]
  );

  return result.rows[0];
};


// Save OTP
exports.saveOtp = async (
  userId,
  otp,
  expiresAt,
  method
) => {
  await pool.query(
    `
    INSERT INTO login_otps (
      user_id,
      otp,
      expires_at,
      method
    )
    VALUES ($1, $2, $3, $4)
    `,
    [
      userId,
      otp,
      expiresAt,
      method,
    ]
  );
};


// Get latest OTP
exports.getLatestOtp = async (
  userId,
  method
) => {
  const result = await pool.query(
    `
    SELECT *
    FROM login_otps
    WHERE user_id = $1
      AND method = $2
    ORDER BY created_at DESC
    LIMIT 1
    `,
    [
      userId,
      method,
    ]
  );

  return result.rows[0];
};


// Check if OTP can be resent
exports.canResendOtp = async (
  userId,
  method
) => {
  const result = await pool.query(
    `
    SELECT created_at
    FROM login_otps
    WHERE user_id = $1
      AND method = $2
    ORDER BY created_at DESC
    LIMIT 1
    `,
    [
      userId,
      method,
    ]
  );

  if (!result.rows[0]) {
    return true;
  }

  const lastSentAt = new Date(
    result.rows[0].created_at
  );

  const elapsedSeconds =
    (Date.now() - lastSentAt.getTime()) / 1000;

  return elapsedSeconds >= 90;
};


// Get OTP resend time
exports.getResendTime = async (
  userId,
  method
) => {
  const result = await pool.query(
    `
    SELECT created_at
    FROM login_otps
    WHERE user_id = $1
      AND method = $2
    ORDER BY created_at DESC
    LIMIT 1
    `,
    [
      userId,
      method,
    ]
  );

  if (!result.rows[0]) {
    return 0;
  }

  const lastSentAt = new Date(
    result.rows[0].created_at
  );

  const elapsedSeconds =
    (Date.now() - lastSentAt.getTime()) / 1000;

  return Math.max(
    0,
    Math.ceil(90 - elapsedSeconds)
  );
};


// Delete OTP
exports.deleteOtp = async (
  userId,
  method
) => {
  await pool.query(
    `
    DELETE FROM login_otps
    WHERE user_id = $1
      AND method = $2
    `,
    [
      userId,
      method,
    ]
  );
};