package nz.geek.nathan.minesweeper.splash

import android.app.Activity
import android.support.v4.app.ActivityCompat.startActivityForResult
import com.firebase.ui.auth.AuthUI
import com.google.firebase.auth.FirebaseAuth

/**
 * Created by nate on 17/05/17.
 */
class SplashPresenter(val mView:SplashContract.View): SplashContract.Presenter {
    override fun start() {
        if(FirebaseAuth.getInstance().currentUser == null){
            (mView as Activity).startActivityForResult(
                    // Get an instance of AuthUI based on the default app
                    AuthUI.getInstance().createSignInIntentBuilder().build(),
                    1234)
        }
    }

    override fun stop() {
        TODO("not implemented") //To change body of created functions use File | Settings | File Templates.
    }
}