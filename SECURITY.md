# Security Policy

## 🔒 Reporting a Vulnerability

**We take security seriously.** If you discover a security vulnerability in aiMate, please help us by reporting it responsibly.

### ⚠️ Please Do NOT:

- **Open a public GitHub issue** for security vulnerabilities
- **Disclose the vulnerability publicly** before we've had a chance to address it
- **Exploit the vulnerability** beyond what's necessary to demonstrate it

### ✅ Please DO:

**Report security vulnerabilities privately to:**

📧 **Email:** security@aimate.co.nz

**Include in your report:**

1. **Description** - Clear explanation of the vulnerability
2. **Impact** - What an attacker could do with this vulnerability
3. **Steps to Reproduce** - Detailed steps to reproduce the issue
4. **Affected Versions** - Which versions are vulnerable
5. **Suggested Fix** - If you have ideas on how to fix it (optional)
6. **Your Contact Info** - So we can follow up with questions

### 📅 What to Expect

**Our Response Timeline:**

- **Within 48 hours:** We'll acknowledge receipt of your report
- **Within 7 days:** We'll provide an initial assessment and plan
- **Within 30 days:** We'll aim to release a fix (timeline depends on severity)

**We will:**
- Keep you informed of our progress
- Credit you in the security advisory (unless you prefer to remain anonymous)
- Work with you to understand and validate the vulnerability
- Notify you when the fix is released

### 🏆 Recognition

We appreciate security researchers who help keep aiMate safe! Contributors who responsibly disclose vulnerabilities will be:

- **Credited** in the security advisory (if desired)
- **Acknowledged** in our CHANGELOG
- **Thanked** publicly (with your permission)

We don't currently have a bug bounty program, but we deeply value your contributions to the security of aiMate.

## 🛡️ Security Best Practices

### For Users/Operators

**When deploying aiMate:**

1. **Keep Software Updated**
   - Always run the latest version of aiMate
   - Keep .NET, PostgreSQL, Docker, and all dependencies updated
   - Subscribe to security advisories via GitHub Watch

2. **Secure Your Environment**
   - Change default passwords immediately
   - Use strong, unique passwords (minimum 16 characters)
   - Generate secure JWT secrets (32+ characters)
   - Enable firewall rules (UFW, iptables, cloud security groups)
   - Use HTTPS/TLS for all connections
   - Restrict database access to localhost or trusted IPs

3. **Configuration Security**
   - Never commit `.env` files to version control
   - Rotate secrets regularly (API keys, JWT secrets, passwords)
   - Use environment-specific secrets (dev vs. production)
   - Enable rate limiting (already configured in nginx)
   - Review and restrict CORS settings
   - Disable debug mode in production

4. **Database Security**
   - Use strong PostgreSQL passwords
   - Restrict PostgreSQL access to localhost
   - Enable SSL/TLS for database connections
   - Regular backups (use the provided backup script)
   - Keep PostgreSQL updated

5. **API Security**
   - Rotate API keys regularly
   - Revoke unused API keys
   - Monitor API usage for anomalies
   - Use rate limiting per API key
   - Log all API access

6. **Monitoring**
   - Review logs regularly (`docker-compose logs`)
   - Set up alerts for suspicious activity
   - Monitor resource usage
   - Track failed authentication attempts

### For Developers

**When contributing code:**

1. **Input Validation**
   - Validate all user input
   - Sanitize data before database operations
   - Use parameterized queries (Entity Framework handles this)
   - Validate file uploads (type, size, content)

2. **Authentication & Authorization**
   - Never store passwords in plaintext
   - Use BCrypt for password hashing (we do)
   - Validate JWT tokens on every request
   - Check user permissions before operations
   - Implement proper session management

3. **Data Protection**
   - Don't log sensitive data (passwords, API keys, PII)
   - Use HTTPS for all external communications
   - Encrypt sensitive data at rest (if applicable)
   - Follow GDPR/privacy best practices

4. **Dependencies**
   - Keep NuGet packages updated
   - Review security advisories for dependencies
   - Use `dotnet list package --vulnerable` regularly
   - Avoid packages with known vulnerabilities

5. **Code Quality**
   - Follow secure coding practices
   - Use static analysis tools
   - Review code for common vulnerabilities:
     - SQL Injection (prevented by EF Core)
     - XSS (sanitize user content)
     - CSRF (use anti-forgery tokens)
     - Command Injection
     - Path Traversal
   - Write security-focused tests

## 🔐 Known Security Considerations

### Current Implementation

**What We've Done:**

- ✅ BCrypt password hashing (work factor: 12)
- ✅ JWT authentication with secure secrets
- ✅ API key hashing (never stored in plaintext)
- ✅ Rate limiting (nginx configuration)
- ✅ Input validation on API endpoints
- ✅ Parameterized database queries (EF Core)
- ✅ HTTPS enforcement (production nginx config)
- ✅ CORS configuration
- ✅ File upload size limits (10MB)
- ✅ Structured logging (no sensitive data)

**Areas for Additional Hardening:**

- ⚠️ Two-factor authentication (planned)
- ⚠️ Advanced rate limiting per user/API key
- ⚠️ Content Security Policy headers
- ⚠️ Security headers (X-Frame-Options, etc.)
- ⚠️ Automated security scanning in CI/CD
- ⚠️ Penetration testing

## 📚 Security Resources

**Relevant Documentation:**

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [.NET Security Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/security/)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [PostgreSQL Security](https://www.postgresql.org/docs/current/security.html)
- [Docker Security](https://docs.docker.com/engine/security/)

## 🔄 Security Updates

**We publish security advisories:**

- Via [GitHub Security Advisories](https://github.com/ChoonForge/aiMate/security/advisories)
- In our release notes and documentation
- On our website (when available)

**To stay informed:**

1. Watch this repository (click "Watch" → "All Activity")
2. Enable "Security alerts" in your GitHub notification settings
3. Subscribe to releases
4. Follow us on social media (coming soon)

## 🙏 Thank You

**Security is a team effort.** Thank you for helping keep aiMate and our community safe.

If you have questions about security, reach out to security@aimate.co.nz.

---

**Built with ❤️ and security in mind** 🔒🇳🇿
