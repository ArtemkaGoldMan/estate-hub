import './StaticPages.css';

export const TermsPage = () => {
  return (
    <div className="static-page">
      <div className="static-page__container">
        <h1 className="static-page__title">Terms of Service</h1>
        <div className="static-page__content">
          <section>
            <h2>1. Acceptance of Terms</h2>
            <p>
              By accessing and using EstateHub, you accept and agree to be bound by the terms and provision of
              this agreement. If you do not agree to these terms, please do not use our service.
            </p>
          </section>

          <section>
            <h2>2. Use License</h2>
            <p>
              Permission is granted to temporarily access EstateHub for personal, non-commercial transitory
              viewing only. This is the grant of a license, not a transfer of title, and under this license you
              may not:
            </p>
            <ul>
              <li>Modify or copy the materials</li>
              <li>Use the materials for any commercial purpose or for any public display</li>
              <li>Attempt to reverse engineer any software contained on EstateHub</li>
              <li>Remove any copyright or other proprietary notations from the materials</li>
            </ul>
          </section>

          <section>
            <h2>3. User Accounts</h2>
            <p>
              You are responsible for maintaining the confidentiality of your account and password. You agree to
              accept responsibility for all activities that occur under your account or password.
            </p>
          </section>

          <section>
            <h2>4. Listing Content</h2>
            <p>
              Users who post listings are responsible for the accuracy and legality of their content. EstateHub
              reserves the right to remove any listing that violates our policies or applicable laws.
            </p>
          </section>

          <section>
            <h2>5. Prohibited Uses</h2>
            <p>You may not use EstateHub:</p>
            <ul>
              <li>In any way that violates any applicable national or international law or regulation</li>
              <li>To transmit, or procure the sending of, any advertising or promotional material</li>
              <li>To impersonate or attempt to impersonate the company, a company employee, another user, or any
                other person or entity</li>
              <li>In any way that infringes upon the rights of others</li>
            </ul>
          </section>

          <section>
            <h2>6. Disclaimer</h2>
            <p>
              The materials on EstateHub are provided on an 'as is' basis. EstateHub makes no warranties,
              expressed or implied, and hereby disclaims and negates all other warranties including, without
              limitation, implied warranties or conditions of merchantability, fitness for a particular purpose,
              or non-infringement of intellectual property or other violation of rights.
            </p>
          </section>

          <section>
            <h2>7. Limitations</h2>
            <p>
              In no event shall EstateHub or its suppliers be liable for any damages (including, without
              limitation, damages for loss of data or profit, or due to business interruption) arising out of the
              use or inability to use the materials on EstateHub.
            </p>
          </section>

          <section>
            <h2>8. Revisions</h2>
            <p>
              EstateHub may revise these terms of service at any time without notice. By using this website you
              are agreeing to be bound by the then current version of these terms of service.
            </p>
          </section>

          <section>
            <h2>9. Contact Information</h2>
            <p>
              If you have any questions about these Terms of Service, please contact us at
              legal@estatehub.com.
            </p>
          </section>
        </div>
      </div>
    </div>
  );
};

